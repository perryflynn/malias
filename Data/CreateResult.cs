namespace maliasmgr.Data
{
    /// <summary>
    /// Result of a create email alias request
    /// </summary>
    public enum CreateResult
    {
        Success = 1,
        Fail = 2,
        AlreadyExists = 4
    }
}
