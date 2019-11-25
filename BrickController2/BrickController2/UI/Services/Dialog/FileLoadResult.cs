namespace BrickController2.UI.Services.Dialog
{
    public class FileLoadResult<T>
    {
        public FileLoadResult(bool isOk, T result)
        {
            IsOk = isOk;
            Result = result;
        }

        public FileLoadResult(bool isOk) : this (isOk, default)
        {
        }

        public bool IsOk { get; }
        public T Result { get; }
    }
}
