using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
//using Microsoft.ADTD.Net;

namespace Microsoft.ADTD.Data
{
    public class TestInfo
    {
        private string _FileName;
        public string FileName
        {
            get { return _FileName; }
        }

        private DateTime _TimeStramp;
        public DateTime TimeStamp
        {
            get { return _TimeStramp; }
        }

        public TestInfo(string FName)
        {
            _FileName = FName;
            readData();
        }

        private void readData()
        {
            XmlDocument myXml = new XmlDocument();

            myXml.Load(_FileName);
            try
            {
                _TimeStramp = Convert.ToDateTime(myXml.SelectSingleNode("/WorkItem").Attributes["TimeEnded"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //Logger.logDebug(ex.ToString());
            }
        }
    }
}
