using System;
using System.Globalization;

namespace Editor {
    public struct TileItem {
        private static CultureInfo FORMAT = new CultureInfo("en-US");
        
        private const int ValueCount = 10;

        public string ModelFile;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float ScaleFactor;
        public string ModelId;
        public string Type;

        public static TileItem FromCsv(string line) {
            string[] values = line.Split(';');
            if (values.Length != ValueCount) {
                throw new ArgumentException("CSV line '" + line + "' does not contain exactly " + ValueCount +
                                            " values!");
            }

            TileItem tileItem = new TileItem();
            tileItem.ModelFile = values[0];
            tileItem.PositionX = Convert.ToSingle(values[1], FORMAT);
            tileItem.PositionY = Convert.ToSingle(values[2], FORMAT);
            tileItem.PositionZ = Convert.ToSingle(values[3], FORMAT);

            tileItem.RotationX = Convert.ToSingle(values[4], FORMAT);
            tileItem.RotationY = Convert.ToSingle(values[5], FORMAT);
            tileItem.RotationZ = Convert.ToSingle(values[6], FORMAT);
            
            tileItem.ScaleFactor = Convert.ToSingle(values[7], FORMAT);

            tileItem.ModelId = values[8];
            
            tileItem.Type = values[9];
            
            return tileItem;
        }
    }
}