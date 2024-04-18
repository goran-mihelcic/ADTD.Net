using Mihelcic.Net.Visio.Arrange;
using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Data
{
    public class dxLayoutParameters
    {
        #region Public Properties

        public int AttractionFactor { get; protected set; }
        public int RepulsionFactor { get; protected set; }
        public int InitialLength { get; protected set; }
        public int HorizontalDistance { get; protected set; }
        public int VerticalDistance { get; protected set; }
        public int LeftMargin { get; protected set; }
        public int RightMargin { get; protected set; }
        public int TopMargin { get; protected set; }
        public int BottomMargin { get; protected set; }
        public bool LaveFirstEmpty { get; protected set; }

        #endregion

        public dxLayoutParameters()
        {
            this.AttractionFactor = 50;
            this.RepulsionFactor = 90;
            this.InitialLength = 200;
            this.HorizontalDistance = 10;
            this.VerticalDistance = 10;
            this.LeftMargin = 25;
            this.RightMargin = 10;
            this.TopMargin = 20;
            this.BottomMargin = 10;
            this.LaveFirstEmpty = true;
        }

        #region Public Methods

        public Dictionary<LayoutParameters, object> GetLayoutParameters()
        {
            Dictionary<LayoutParameters, object> result = new Dictionary<LayoutParameters, object>();

            result.Add(LayoutParameters.AttractionFactor, this.AttractionFactor);
            result.Add(LayoutParameters.RepulsionFactor, this.RepulsionFactor);
            result.Add(LayoutParameters.InitialLength, this.InitialLength);
            result.Add(LayoutParameters.HorizontalDistance, this.HorizontalDistance);
            result.Add(LayoutParameters.VerticalDistance, this.VerticalDistance);
            result.Add(LayoutParameters.LeftMargin, this.LeftMargin);
            result.Add(LayoutParameters.RightMargin, this.RightMargin);
            result.Add(LayoutParameters.TopMargin, this.TopMargin);
            result.Add(LayoutParameters.BottomMargin, this.BottomMargin);
            result.Add(LayoutParameters.LaveFirstEmpty, this.LaveFirstEmpty);

            return result;
        }

        #endregion
    }
}
