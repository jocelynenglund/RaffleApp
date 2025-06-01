using RaffleDraw.Domain.Ports;

namespace RaffleDraw.Domain.Services;

public class RandomWinnerSelector: IWinnerSelector
{
    private readonly Random _rng;
    public RandomWinnerSelector(int? seed = null)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public int ChooseWinner(IReadOnlyList<int> holders)
    {
        if (holders.Count == 0)
            throw new InvalidOperationException("No holders to pick from");

        int index = _rng.Next(holders.Count);
        return holders[index];
    }
}