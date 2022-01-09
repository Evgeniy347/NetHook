namespace NetHook.Cores.Inject
{
    public interface IDomainEntryPoint
    {
        void InjectDomain(string inChannelName);
    }
}
