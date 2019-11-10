using System;
using System.Globalization;

namespace Editor
{
    public struct WmoItem
    {
        private static CultureInfo FORMAT = new CultureInfo("en-US");
        
        private const int ValueCount = 10;
        
        public string ModelFile;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationW;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float ScaleFactor;
        public string DoodadSet;
        
        public static WmoItem FromCsv(string line)
        {
            string[] values = line.Split(';');
            if (values.Length != ValueCount)
            {
                throw new ArgumentException("CSV line '" + line + "' does not contain exactly " + ValueCount + " values!");
            }
            
            WmoItem wmoItem = new WmoItem();
            wmoItem.ModelFile = values[0];
            wmoItem.PositionX = Convert.ToSingle(values[1], FORMAT);
            wmoItem.PositionY = Convert.ToSingle(values[2], FORMAT);
            wmoItem.PositionZ = Convert.ToSingle(values[3], FORMAT);
            
            wmoItem.RotationW = Convert.ToSingle(values[4], FORMAT);
            wmoItem.RotationX = Convert.ToSingle(values[5], FORMAT);
            wmoItem.RotationY = Convert.ToSingle(values[6], FORMAT);
            wmoItem.RotationZ = Convert.ToSingle(values[7], FORMAT);
            
            wmoItem.ScaleFactor = Convert.ToSingle(values[8], FORMAT);
            
            wmoItem.DoodadSet = values[9];

            return wmoItem;
        }
    }
}