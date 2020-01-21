using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameControl
{
    /// <summary>
    /// Represents ship spawned on the grid
    /// </summary>
    public class Ship
    {
        /// <summary>
        /// Cells on which ship is placed
        /// </summary>
        public readonly IReadOnlyList<int> cells;
        /// <summary>
        /// <see langword="true"/> after ship has been destroyed
        /// </summary>
        public bool isDestroyed { get; private set; }
        /// <summary>
        /// Number of cells taken by ship
        /// </summary>
        public int Size => cells.Count;

        public Ship(params int[] cellNumbers)
        {
            cells = Array.AsReadOnly(cellNumbers);
            isDestroyed = false;
        }

        /// <summary>
        /// Marks <see cref="Ship"/> as destroyed
        /// </summary>
        public void Destroy()
        {
            if (isDestroyed) throw new InvalidOperationException($"Ship positioned on cells: {cells.Aggregate("", (p, n) => p + " " + n.ToString())} was already destroyed");

            isDestroyed = true;
        }

        public override int GetHashCode()
        {
            int hashCode = int.MinValue;
            for (int i = 0; i < cells.Count; i++)
            {
                hashCode += cells[i] * i;
            }
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if(obj is Ship)
            {
                return ((Ship)obj).cells.Equals(this.cells);
            }
            else
            {
                return false;
            }
        }
    }
}
