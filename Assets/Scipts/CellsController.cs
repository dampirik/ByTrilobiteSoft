using UnityEngine;

namespace Assets.Scipts
{
    public class CellsController
    {
        private readonly byte[][] _cells;

        private readonly int _offset;

        private readonly int _size;

        public CellsController(int size)
        {
            _size = size;
            _offset = _size / 2;

            Debug.Log("size: " + size + " _offset: " + _offset);

            _cells = new byte[size][];

            for (var x = 0; x < size; x++)
            {
                _cells[x] = new byte[size];
            }
        }

        private void Convert(ref int x, ref int y)
        {
            x = x + _offset;
            y = y + _offset;

            if (_size%2==0)
            {
                x -= 1;
                y -= 1;
            }
        }

        public bool? CheckFree(int x, int y)
        {
            Convert(ref x, ref y);

            if (x < 0 || x >= _cells.Length)
            {
                return null;
            }

            if (y < 0 || y >= _cells[x].Length)
            {
                return null;
            }

            return _cells[x][y] == 0;
        }

        public void SetBusy(int x, int y)
        {
            Convert(ref x, ref y);

            _cells[x][y] = 1;
        }

        public void SetFree(int x, int y)
        {
            Convert(ref x, ref y);

            _cells[x][y] = 0;
        }
    }
}
