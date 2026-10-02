using Dem2Krit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dem2Krit
{
    internal class Helper
    {
        private static Dem2KritEntities _context;
        public static Dem2KritEntities GetContext()
        {
            if (_context == null)
            {
                _context = new Dem2KritEntities();
            }
            return _context;
        }
    }
}
