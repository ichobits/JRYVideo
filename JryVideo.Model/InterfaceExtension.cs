namespace JryVideo.Model
{
    public static class InterfaceExtension
    {
        public static string GetValidImdb(this IImdbItem item)
            => item?.ImdbId != null && item.ImdbId.StartsWith("tt") ? item.ImdbId : null;
    }
}