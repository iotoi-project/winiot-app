using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdapterLib;


namespace CommaxIoTApp.Utils
{
    public class ZigBee
    {
        private Adapter m_zigBeeAdapter = null;        

        public void Test()
        {
            m_zigBeeAdapter = new Adapter();
            m_zigBeeAdapter.Initialize();
            
        }

    }
}
