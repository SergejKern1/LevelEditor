namespace Level.PlatformLayer.Interface
{
    public interface IPlatformLayerTiles
    {
        ushort this[int index] { get; set; }
        ushort[] Tiles { get; }

        void SetTiles(ushort[] tiles);
    }
}