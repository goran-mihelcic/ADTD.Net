using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Matrix for organizing shapes in rows and columns
    /// </summary>
    internal class ShapeMatrix
    {
        private Double _hspace = 0; //22.5;
        private Double _vSpace = 0; //17.5;
        private Double _leftMargin = 0; //22.5;
        private Double _rightmargin = 0; //5;
        private Double _topMargin = 0; //17.5;
        private Double _bottomMargin = 0; //5;
        private Double _aspect = 0;
        private bool _leaveFirstEmpty = false;

        private readonly bool _isHierarchy;

        private readonly Dictionary<Int32, Dictionary<Int32, IDiagramNode>> _rowsXColumns;
        
        /// <summary>
        /// Rows x Colors
        /// </summary>
        public Dictionary<Int32, Dictionary<Int32, IDiagramNode>> Rows { get { return _rowsXColumns; } }

        private readonly Dictionary<Int32, Dictionary<Int32, IDiagramNode>> _columnsXRows;


        private Int32 _count;
        
        /// <summary>
        /// Shape count in the Matrix
        /// </summary>
        internal Int32 Count { get { return _count; } }

        /// <summary>
        /// Construct new Matrix
        /// </summary>
        /// <param name="isHierarchy">Do we use Matrix for creating hierarchy</param>
        internal ShapeMatrix(bool isHierarchy = false)
        {
            _isHierarchy = isHierarchy;
            _rowsXColumns = new Dictionary<int, Dictionary<int, IDiagramNode>>();
            _columnsXRows = new Dictionary<int, Dictionary<int, IDiagramNode>>();
            _count = 0;
        }

        /// <summary>
        /// Adds Diagram nodes from the supplied IEnumerable to the Matrix such way to create matrix  
        /// with nomber of rows == number of columns
        /// </summary>
        /// <param name="nodes">Diagram nodes to add to Matrix</param>
        internal void AddShapes(IEnumerable<IDiagramNode> nodes)
        {
            Int32 i = 0;
            int count = nodes.Count();

            foreach (IDiagramNode node in nodes)
            {
                i++;
                AddShape(i, count, node);
            }
        }

        /// <summary>
        /// Add shapes to Matrix to create Matrix with fixed column number
        /// </summary>
        /// <param name="nodes">Diagram nodes to add to Matrix</param>
        /// <param name="columns">Column number</param>
        internal void AddShapesWithFixColumns(IEnumerable<IDiagramNode> nodes, Int32 columns)
        {
            Int32 i = 0;
            int count = nodes.Count();

            foreach (IDiagramNode node in nodes)
            {
                int column = i % columns;
                int row = Convert.ToInt32(Math.Truncate(Convert.ToDouble(i) / columns));
                i++;
                AddShape(node, row, column);
            }
        }

        /// <summary>
        /// Add shapes to Matrix to create Matrix with fixed row number
        /// </summary>
        /// <param name="nodes">Diagram nodes to add to Matrix</param>
        /// <param name="rows">Row Number</param>
        internal void AddShapesWithFixRows(IEnumerable<IDiagramNode> nodes, Int32 rows)
        {
            Int32 i = 0;
            int count = nodes.Count();

            foreach (IDiagramNode node in nodes)
            {
                int row = i % rows;
                int column = Convert.ToInt32(Math.Truncate(Convert.ToDouble(i) / rows));
                i++;
                AddShape(node, row, column);
            }
        }

        private void AddShape(int i, int ServerNum, IDiagramNode node)
        {
            int factor;
            int posX;
            int posY;

            if (this._leaveFirstEmpty)
            {
                factor = Convert.ToInt32(Math.Truncate(Math.Sqrt(ServerNum) + 1));
                posX = i % factor;
                posY = Convert.ToInt32(Math.Truncate(Convert.ToDouble(i) / factor));
            }
            else
            {
                factor = Convert.ToInt32(Math.Round(Math.Sqrt(ServerNum), 0, MidpointRounding.AwayFromZero));
                posY = (i - 1) % factor;
                posX = Convert.ToInt32(Math.Truncate(Convert.ToDouble(i - 1) / factor));
            }

            AddShape(node, posY, posX);
        }

        /// <summary>
        /// Fill matrix with Diagram Nodes creating spanning tree (hierarchy)
        /// </summary>
        /// <param name="node">Hierarchy root node</param>
        /// <param name="horizontal">Is hierarchy horizontal</param>
        internal void AddHierarchy(HierarchyMemberNode node, bool horizontal)
        {
            AddHierarchy(node, horizontal, 0, 0);
        }
        
        private void AddHierarchy(HierarchyMemberNode node, bool horizontal, int firstDimension, int secondDimension)
        {
            Int32 rowNum;
            Int32 columnNum;

            rowNum = secondDimension;
            columnNum = firstDimension;

            if (horizontal)
            {
                AddShape(node, rowNum, columnNum);

                for (int i = 0; i < node.Children.Count; i++)
                {
                    HierarchyMemberNode child = node.Children[i];
                    if (i == 0)
                        rowNum++;
                    else
                    {
                        bool notFound = true;
                        while (notFound)
                        {
                            columnNum += 2;
                            if (!_columnsXRows.ContainsKey(columnNum))
                                notFound = false;
                        }
                    }
                    AddHierarchy(child, horizontal, columnNum, rowNum);
                }
            }
            else
            {
                AddShape(node, rowNum, columnNum);

                for (int i = 0; i < node.Children.Count; i++)
                {
                    HierarchyMemberNode child = node.Children[i];
                    if (i == 0)
                        columnNum++;
                    else
                    {
                        bool notFound = true;
                        while (notFound)
                        {
                            rowNum += 2;
                            if (!_rowsXColumns.ContainsKey(rowNum))
                                notFound = false;
                        }
                    }
                    AddHierarchy(child, horizontal, columnNum, rowNum);
                }
            }
        }

        /// <summary>
        /// Place parents in central position in the hierarchy
        /// </summary>
        /// <param name="node">Hierarchy root node</param>
        /// <param name="horizontal">Is hierarchy horizontal</param>
        internal void CenterHierarchy(HierarchyMemberNode node, bool horizontal)
        {
            Int32 origCol = GetHierarchyNodeColumn(node);
            Int32 origRow = GetHierarchyNodeRow(node);

            foreach (HierarchyMemberNode child in node.Children)
                CenterHierarchy(child, horizontal);

            if (node.Children.Count > 1)
            {
                Int32 newCol = origCol;
                Int32 newRow = origRow;

                if (horizontal)
                {
                    Double cSum = node.Children.Sum(c => GetHierarchyNodeColumn(c));
                    newCol = Convert.ToInt32(cSum / node.Children.Count);
                }
                else
                {
                    Double rSum = node.Children.Sum(c => GetHierarchyNodeRow(c));
                    newRow = Convert.ToInt32(Math.Round(rSum / node.Children.Count));
                }

                MoveHierarchyNode(node, newCol, newRow, true);
            }
        }

        internal void RemoveEmptySpace()
        {
            Int32 maxCol = _columnsXRows.Max(c => c.Key);
            Int32 maxRow = _rowsXColumns.Max(r => r.Key);

            // Remove empty Columns
            for (Int32 empty = maxCol; empty >= 0; empty--)
            {
                if (!_columnsXRows.ContainsKey(empty))
                {
                    for (Int32 moveCol = empty + 1; moveCol <= maxCol; moveCol++)
                    {
                        List<Int32> rows = _columnsXRows[moveCol].Keys.ToList();
                        foreach (Int32 row in rows)
                        {
                            if (_columnsXRows[moveCol].ContainsKey(row))
                                MoveHierarchyNode(_columnsXRows[moveCol][row] as HierarchyMemberNode, moveCol - 1, row);
                        }
                        _columnsXRows.Remove(moveCol);
                        maxCol = _columnsXRows.Max(c => c.Key);
                    }
                }
            }

            // Remove empty rows
            for (Int32 empty = maxRow; empty >= 0; empty--)
            {
                if (!_rowsXColumns.ContainsKey(empty))
                {
                    for (Int32 moveRow = empty + 1; moveRow <= maxCol; moveRow++)
                    {
                        List<Int32> columns = _rowsXColumns[moveRow].Keys.ToList();
                        foreach (Int32 col in columns)
                        {
                            if (_rowsXColumns[moveRow].ContainsKey(col))
                                MoveHierarchyNode(_rowsXColumns[moveRow][col] as HierarchyMemberNode, col, moveRow - 1);
                        }
                        _rowsXColumns.Remove(moveRow);
                        maxRow = _rowsXColumns.Max(r => r.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Set Aspect ratio of the hierarchy
        /// </summary>
        internal void SetAspect()
        {
            if (_aspect != 0)
                if (this.GetWidth(0) * _aspect > this.GetHeight(0))
                {
                    Double hDif = this.GetWidth(0) - this.GetHeight(0);
                    Double extension = hDif / (_rowsXColumns.Count - 1);
                    _vSpace += extension;
                }
                else
                {
                    Double vDif = this.GetHeight(0) - this.GetWidth(0);
                    Double extension = vDif / (_columnsXRows.Count - 1);
                    _hspace += extension;
                }
        }

        private void MoveHierarchyNode(HierarchyMemberNode node, Int32 column, Int32 row, bool moveParent = false)
        {
            Int32 origCol = GetHierarchyNodeColumn(node);
            Int32 origRow = GetHierarchyNodeRow(node);

            Int32 deltaCol = column - origCol;
            Int32 deltaRow = row - origRow;

            _rowsXColumns[origRow].Remove(origCol);
            _columnsXRows[origCol].Remove(origRow);

            AddShape(node, row, column);

            if (moveParent && node.HParent != null)
            {
                Int32 origParentCol = GetHierarchyNodeColumn(node.HParent);
                Int32 origParentRow = GetHierarchyNodeRow(node.HParent);
                MoveHierarchyNode(node.HParent, origParentCol + deltaCol, origParentRow + deltaRow);
            }
        }

        /// <summary>
        /// Returns Column where the Node is placed
        /// </summary>
        /// <param name="node">Node to find</param>
        /// <returns>Column where the Node is placed or -1 for not found</returns>
        internal Int32 GetHierarchyNodeColumn(HierarchyMemberNode node)
        {
            Int32 result = -1;

            Dictionary<Int32, IDiagramNode> row = _rowsXColumns.FirstOrDefault(r => r.Value.ContainsValue(node)).Value;
            if (row != null)
            {
                if (row.Any(c => c.Value == node))
                    result = row.First(c => c.Value == node).Key;
            }

            return result;
        }

        /// <summary>
        /// Returns Row where the Node is placed
        /// </summary>
        /// <param name="node">Node to find</param>
        /// <returns>Row where the Node is placed or -1 for not found</returns>
        internal Int32 GetHierarchyNodeRow(HierarchyMemberNode node)
        {
            Int32 result = -1;

            Dictionary<Int32, IDiagramNode> col = _columnsXRows.FirstOrDefault(r => r.Value.ContainsValue(node)).Value;
            if (col != null)
            {
                if (col.Any(c => c.Value == node))
                    result = col.First(c => c.Value == node).Key;
            }

            return result;
        }

        private void AddShape(IDiagramNode node, Int32 rowNum, Int32 columnNum)
        {
            Dictionary<int, IDiagramNode> myColumn;
            Dictionary<int, IDiagramNode> myRow;

            if (_rowsXColumns.ContainsKey(rowNum))
                myRow = _rowsXColumns[rowNum];
            else
            {
                myRow = new Dictionary<int, IDiagramNode>();
                _rowsXColumns.Add(rowNum, myRow);
            }
            myRow.Add(columnNum, node);

            if (_columnsXRows.ContainsKey(columnNum))
                myColumn = _columnsXRows[columnNum];
            else
            {
                myColumn = new Dictionary<int, IDiagramNode>();
                _columnsXRows.Add(columnNum, myColumn);
            }
            myColumn.Add(rowNum, node);
            _count++;
        }

        /// <summary>
        /// Returns hight of the row
        /// </summary>
        /// <param name="rowNum">Row number</param>
        /// <returns>Row height or 0 for non existing row</returns>
        internal Double GetRowHeight(Int32 rowNum)
        {
            if (_rowsXColumns.ContainsKey(rowNum))
                return _rowsXColumns[rowNum].Values.Max(n => n.Height);
            return 0;
        }

        /// <summary>
        /// Returns width of the column
        /// </summary>
        /// <param name="columnNum">Column number</param>
        /// <returns>Column width or 0 for non existing column</returns>
        internal Double GetColumnWidth(Int32 columnNum)
        {
            if (_columnsXRows.ContainsKey(columnNum))
                return _columnsXRows[columnNum].Values.Max(n => n.Width);
            return 0;
        }

        /// <summary>
        /// Returns left X coordinate of the column
        /// </summary>
        /// <param name="columnNum">Column number</param>
        /// <returns>Left X coordinate of the column</returns>
        internal Double GetColumnLeft(Int32 columnNum)
        {
            if (columnNum == 0)
                return _leftMargin;
            else
            {
                return GetColumnRight(columnNum - 1) + _hspace;
            }
        }

        /// <summary>
        ///  Returns right X coordinate of the column
        /// </summary>
        /// <param name="columnNum">Column number</param>
        /// <returns>Right X coordinate of the column</returns>
        internal Double GetColumnRight(Int32 columnNum)
        {
            return GetColumnLeft(columnNum) + GetColumnWidth(columnNum);
        }

        /// <summary>
        /// Returns Top Y coordinate of the row
        /// </summary>
        /// <param name="rowNum">Row number</param>
        /// <returns>Top Y coordinate of the row</returns>
        internal Double GetRowTop(Int32 rowNum)
        {
            if (rowNum == 0)
                return _topMargin;
            else
            {
                return GetRowBottom(rowNum - 1) + _vSpace;
            }
        }

        /// <summary>
        /// Returns Bottom Y coordinate of the row
        /// </summary>
        /// <param name="rowNum">Row number</param>
        /// <returns>Bottom Y coordinate of the row</returns>
        internal Double GetRowBottom(Int32 rowNum)
        {
            return GetRowTop(rowNum) + GetRowHeight(rowNum);
        }

        /// <summary>
        /// Get Matrix total width
        /// </summary>
        /// <param name="width">Value to return if Matrix is empty</param>
        /// <returns>Matrix total width</returns>
        internal Double GetWidth(Double width)
        {
            if (_columnsXRows.Keys.Count > 0)
                if (_isHierarchy)
                    return _leftMargin + GetColumnRight(_columnsXRows.Keys.Max()) + _rightmargin;
                else
                    return GetColumnRight(_columnsXRows.Keys.Max()) + _rightmargin;
            return width;
        }

        /// <summary>
        /// Get Matrix total height
        /// </summary>
        /// <param name="height">Value to return if Matrix is empty</param>
        /// <returns>Matrix total height</returns>
        internal Double GetHeight(Double height)
        {
            if (_rowsXColumns.Keys.Count > 0)
                if (_isHierarchy)
                    return _topMargin + GetRowBottom(_rowsXColumns.Keys.Max()) + _bottomMargin;
                else
                    return GetRowBottom(_rowsXColumns.Keys.Max()) + _bottomMargin;
            return height;
        }

        /// <summary>
        /// Set local variables to parameter values
        /// </summary>
        /// <param name="properties">Parameters</param>
        internal void SetParameters(Dictionary<LayoutParameters, object> properties)
        {
            if (properties.ContainsKey(LayoutParameters.HorizontalDistance))
                _hspace = Convert.ToDouble(properties[LayoutParameters.HorizontalDistance]);
            if (properties.ContainsKey(LayoutParameters.VerticalDistance))
                _vSpace = Convert.ToDouble(properties[LayoutParameters.VerticalDistance]);
            if (properties.ContainsKey(LayoutParameters.LeftMargin))
                _leftMargin = Convert.ToDouble(properties[LayoutParameters.LeftMargin]);
            if (properties.ContainsKey(LayoutParameters.RightMargin))
                _rightmargin = Convert.ToDouble(properties[LayoutParameters.RightMargin]);
            if (properties.ContainsKey(LayoutParameters.TopMargin))
                _topMargin = Convert.ToDouble(properties[LayoutParameters.TopMargin]);
            if (properties.ContainsKey(LayoutParameters.BottomMargin))
                _bottomMargin = Convert.ToDouble(properties[LayoutParameters.BottomMargin]);
            if (properties.ContainsKey(LayoutParameters.LaveFirstEmpty))
                _leaveFirstEmpty = Convert.ToBoolean(properties[LayoutParameters.LaveFirstEmpty]);
            if (properties.ContainsKey(LayoutParameters.Aspect))
                _aspect = Convert.ToDouble(properties[LayoutParameters.Aspect]);
        }

        /// <summary>
        /// Fill parameters with values from local variables
        /// </summary>
        /// <returns>Parameters</returns>
        internal Dictionary<LayoutParameters, object> GetParameters()
        {
            Dictionary<LayoutParameters, object> parameters = new Dictionary<LayoutParameters, object>
            {
                { LayoutParameters.HorizontalDistance, _hspace },
                { LayoutParameters.VerticalDistance, _vSpace },
                { LayoutParameters.Aspect, _aspect },
                { LayoutParameters.LeftMargin, _leftMargin },
                { LayoutParameters.RightMargin, _rightmargin },
                { LayoutParameters.TopMargin, _topMargin },
                { LayoutParameters.BottomMargin, _bottomMargin },
                { LayoutParameters.LaveFirstEmpty, _leaveFirstEmpty }
            };

            return parameters;
        }
    }
}
