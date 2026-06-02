namespace Smart_Farm.Application.Exceptions;

public sealed class CropNotFoundException(int cid) : Exception
{
    public int Cid { get; } = cid;
}
