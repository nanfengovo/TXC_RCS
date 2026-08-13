namespace TXC.RCS.Tasks.TM;

public sealed class TmDispatchBuildResult
{
    public TmDispatchBuildResult(TmTaskAddRequest request, string fetchSerial, string putSerial)
    {
        Request = request;
        FetchSerial = fetchSerial;
        PutSerial = putSerial;
    }

    public TmTaskAddRequest Request { get; }
    public string FetchSerial { get; }
    public string PutSerial { get; }
}
