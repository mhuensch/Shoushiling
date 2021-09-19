namespace TRS.CaptureTool
{
    [System.Serializable]
    public class GifFileSettings : CaptureFileSettings
    {
        public override string encoding
        {
            get
            {
                return "gif";
            }
        }

        public override void SetUp(int uniqueId, string saveType = "Gifs")
        {
            base.SetUp(uniqueId, saveType);

            if (setup)
                return;

            useStreamingAssetsPath = true;
        }
    }
}