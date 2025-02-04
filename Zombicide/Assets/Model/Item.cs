using UnityEngine;

namespace Model
{
    public abstract class Item
    {
        protected string name;
        public string Name { get { return name; } }
        public Item(string name)
        {
            this.name = name;
        }
    }

}
