namespace RaffleDraw.Domain.Ports;

public interface IWinnerSelector
{
    int ChooseWinner(IReadOnlyList<int> holders);
}
