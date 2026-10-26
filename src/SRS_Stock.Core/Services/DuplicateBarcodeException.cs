namespace SRS_Stock.Core.Services;

public sealed class DuplicateBarcodeException : Exception
{
    public DuplicateBarcodeException(string barcode)
        : base($"Barcode '{barcode}' is already used.")
    {
        Barcode = barcode;
    }

    public string Barcode { get; }
}
