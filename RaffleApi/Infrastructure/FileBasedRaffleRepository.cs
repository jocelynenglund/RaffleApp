using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using RaffleDraw.Core;
using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Ports;
using RaffleDraw.Features.BuyTicket;
using RaffleDraw.Features.CreateRaffle;
using RaffleDraw.Features.SelectWinner;

namespace RaffleApi.Infrastructure;

public class FileBasedRaffleRepository : IRaffleRepository
{
    private readonly string _storageDirectory;

    public FileBasedRaffleRepository(string storageDirectory)
    {
        if (string.IsNullOrWhiteSpace(storageDirectory))
            throw new ArgumentException(
                "Storage directory must be specified.",
                nameof(storageDirectory)
            );

        _storageDirectory = storageDirectory;
        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
    }

    public async Task SaveAsync(Raffle raffle, CancellationToken cancellationToken)
    {
        // Persist both the state and events for demonstration.
        // Here we persist events only (i.e. event sourcing).
        string filePath = GetFilePath(raffle.Id);
        List<DomainEvent> events = raffle.UncommittedChanges.ToList();

        // Append new events if file exists (for simplicity, reload and append)
        var existingEvents = new List<DomainEvent>();
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            if (!string.IsNullOrWhiteSpace(json))
            {
                existingEvents =
                    JsonSerializer.Deserialize<List<DomainEvent>>(json, GetJsonOptions())
                    ?? new List<DomainEvent>();
            }
        }

        existingEvents.AddRange(events);

        // Serialize back to file
        var serialized = JsonSerializer.Serialize(existingEvents, GetJsonOptions());
        await File.WriteAllTextAsync(filePath, serialized, cancellationToken);

        // Clear uncommitted changes (assuming AggregateRoot clears its uncommitted events)
        raffle.ClearUncommitted();
    }

    public async Task<Raffle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(id);
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        // Deserialize list of events. Note: You may need custom converters if DomainEvent is polymorphic.
        var events = JsonSerializer.Deserialize<List<DomainEvent>>(json, GetJsonOptions());
        if (events == null || events.Count == 0)
            return null;

        return Raffle.LoadFromHistory(events);
    }

    public async Task<IEnumerable<Raffle>> GetAllAsync(CancellationToken cancellationToken)
    {
        var raffles = new List<Raffle>();
        foreach (var file in Directory.GetFiles(_storageDirectory, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                continue;

            var events = JsonSerializer.Deserialize<List<DomainEvent>>(json, GetJsonOptions());
            if (events != null && events.Count > 0)
            {
                var raffle = Raffle.LoadFromHistory(events);
                raffles.Add(raffle);
            }
        }

        return raffles;
    }

    private string GetFilePath(Guid id)
    {
        return Path.Combine(_storageDirectory, $"raffle_{id}.json");
    }

    private JsonSerializerOptions GetJsonOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new DomainEventJsonConverter());
        return options;
    }
}

public class DomainEventJsonConverter : JsonConverter<DomainEvent>
{
    public override DomainEvent Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        // Load the JSON document
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("$type", out var typeProperty))
            {
                throw new JsonException("Missing discriminator property '$type'.");
            }

            var typeName = typeProperty.GetString();
            Type eventType = typeName switch
            {
                nameof(RaffleCreated) => typeof(RaffleCreated),
                nameof(TicketBought) => typeof(TicketBought),
                nameof(WinnerSelected) => typeof(WinnerSelected),
                _ => throw new NotSupportedException($"Event type '{typeName}' is not supported."),
            };

            // Get the raw JSON and deserialize using the determined event type.
            var rawText = root.GetRawText();
            var domainEvent = (DomainEvent)JsonSerializer.Deserialize(rawText, eventType, options)!;
            return domainEvent;
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        DomainEvent value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        // Write a discriminator property; you can change the property name as desired.
        writer.WriteString("$type", value.GetType().Name);

        // Write out all public properties of the event
        foreach (
            PropertyInfo property in value
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        )
        {
            // Skip the "$type" property if it exists in the object itself.
            if (property.Name == "$type")
            {
                continue;
            }

            var propertyValue = property.GetValue(value);
            writer.WritePropertyName(property.Name);
            JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
        }

        writer.WriteEndObject();
    }
}
