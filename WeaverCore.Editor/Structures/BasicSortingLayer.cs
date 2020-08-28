namespace WeaverCore.Editor.Structures
{
	public struct BasicSortingLayer
    {
        public BasicSortingLayer(string name, long id)
        {
            Name = name;
            UniqueID = id;
        }

        public string Name;
        public long UniqueID;
    }
}

