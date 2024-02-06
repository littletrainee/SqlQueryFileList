namespace SqlQueryFileList
{
    public enum ColumnEnum
    {
        NAME,
        SIZE,
        FILE_LAST_WRITE_TIME,
        EXTENSION
    }

    public enum Block
    {
        SELECT,
        FROM,
        WHERE,
        ORDER
    }

    public enum Like
    {
        NONE,
        START
    }
}