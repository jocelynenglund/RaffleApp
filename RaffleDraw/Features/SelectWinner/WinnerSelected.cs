﻿using RaffleDraw.Core;

namespace RaffleDraw.Features.SelectWinner;

public record WinnerSelected(int TicketNumber, Guid RaffleID) : DomainEvent;
