using System;

namespace WS.DomainModelling.DiscriminatedUnion;

public class DiscriminatedUnion
{
    private enum Option
    {
        Geoff,
        BlobBlobBlob
    }

    private readonly Option option;

    private string BlobBlobBlob_Value { get; init; }

    public bool IsGeoff => option == Option.Geoff;
    public bool IsBlobBlobBlob => option == Option.BlobBlobBlob;

    private DiscriminatedUnion(Option option)
    {
        this.option = option;
        BlobBlobBlob_Value = default!;
    }

    public TResult Match<TResult>(Func<TResult> geoffFunc, Func<string, TResult> blobBlobBlobFunc)
    {
        return option switch
        {
            Option.Geoff => geoffFunc(),
            Option.BlobBlobBlob => blobBlobBlobFunc(BlobBlobBlob_Value),
            _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
        };
    }

    public void Switch<TResult>(Action geoffAction, Action<string> blobBlobBlobAction)
    {
        switch (option)
        {
            case Option.Geoff:
                geoffAction();
                return;
            case Option.BlobBlobBlob:
                blobBlobBlobAction(BlobBlobBlob_Value);
                return;
            default:
                throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
        }
    }

    public static DiscriminatedUnion Geoff { get; } = new DiscriminatedUnion(Option.Geoff);

    public static DiscriminatedUnion BlobBlobBlob(string blobblobblob) => new(Option.BlobBlobBlob) { BlobBlobBlob_Value = blobblobblob };
}
