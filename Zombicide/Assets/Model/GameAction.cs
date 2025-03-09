using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class GameAction
    {
        public string Name { get; private set; }
        public int Cost { get; private set; }
        public Action ExecutionMethod { get; private set; }
        public GameAction(string name, int cost, Action executionMethod)
        {
            Name = name;
            Cost = cost;
            ExecutionMethod = executionMethod;
        }
        public void Execute()
        {
            ExecutionMethod.Invoke();
        }
    }
}
