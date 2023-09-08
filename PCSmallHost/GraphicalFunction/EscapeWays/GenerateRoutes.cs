using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using PCSmallHost.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PCSmallHost.GraphicalFunction.EscapeWays
{
    public class GenerateRoutes
    {
        MainWindow _context;

        private bool IsNoLogin = false;
        private int ElementLoginCount = 0;
        private int ELementNoLoginCount = 0;
        private int Pointdistance = 30;
        struct nextLines
        {
            public EscapeLinesInfo nextline;
            public Point StartingPoint;
        };
        public struct AlarmPoint
        {
            public Line FootDropLine;//火灾报警点对应的线段
            public Point Perpendicular;//火灾报警点对应的路线出发点
        };
        public struct AdjoinLine
        {
            public AlarmPoint alarm;
            public Point AdjacentPoint;//两条线段相交的点
        };

        private List<nextLines> OldNextLines = new List<nextLines>();
        private List<nextLines> NewNextLines = new List<nextLines>();
        private List<Point> Corners = new List<Point>();
        private List<Line> AlreadyJudgeLine = new List<Line>();//记录已经判断的线段
        private List<Line> AlreadyDeleLine = new List<Line>();//记录已经删除箭头的线段
        /// <summary>
        /// 火灾报警点对应的线路
        /// </summary>
        private List<AlarmPoint> FootDropLines = new List<AlarmPoint>();
        /// <summary>
        /// 已经生成相应路线的报警点
        /// </summary>
        private List<AlarmPoint> FootDropLinesPrinted = new List<AlarmPoint>();
        /// <summary>
        /// 存放安全出口坐标
        /// </summary>
        private List<Point> Ends = new List<Point>();
        /// <summary>
        /// 当前楼层
        /// </summary>
        private int CurrentFloor = 0;
        /// <summary>
        /// 已经成功绘画出路线方向的线段
        /// </summary>
        private List<Line> PrintedLines = new List<Line>();
        /// <summary>
        /// 新增报警点时相应的线路更改后，保存已经更改的线段
        /// </summary>
        private List<Line> ChaRouteDirection = new List<Line>();

        private List<PlanPartitionPointRecordInfo> LstPlanPartitionPointCurrent = new List<PlanPartitionPointRecordInfo>();
        /// <summary>
        /// 存放路线的出口坐标
        /// </summary>
        private List<EscapeRoutesInfo> EndPoint = new List<EscapeRoutesInfo>();
        /// <summary>
        /// 当前楼层的路线
        /// </summary>
        private List<EscapeLinesInfo> LstEscapeLinesCurrentFloor = new List<EscapeLinesInfo>();

        private List<EscapeRoutesInfo> LstEscapeRoutesCurrentFloor = new List<EscapeRoutesInfo>();

        private List<CoordinateInfo> LstCoordinateCurrentFloor = new List<CoordinateInfo>();

        private List<EscapeRoutesInfo> LstEscapeRoutes = new List<EscapeRoutesInfo>();
        private List<CoordinateInfo> LstCoordinate = new List<CoordinateInfo>();
        private List<PlanPartitionPointRecordInfo> LstPlanPartitionPointRecord = new List<PlanPartitionPointRecordInfo>();

        private CEscapeRoutes ObjEscapeRoutes = new CEscapeRoutes();
        private CCoordinate ObjCoordinate = new CCoordinate();
        private CPlanPartitionPointRecord ObjPlanPartitionPointRecord = new CPlanPartitionPointRecord();

        /// <summary>
        /// 获取当前联动号和当前楼层
        /// </summary>
        public List<Line> GetInformation(int floornum, List<EscapeLinesInfo> lstEscapeLines, List<EscapeRoutesInfo> lstEscapeRoutes, List<CoordinateInfo> lstCoordinate, List<PlanPartitionPointRecordInfo> lstPlanPartitionPointRecord, List<AlarmPoint> footDropLines, bool isNoLogin, MainWindow context)
        {
            // TODO 获取当前联动号和当前楼层
            _context = context;
            ElementLoginCount = _context.cvsMainWindow.Children.Count;
            ELementNoLoginCount = _context.cvsMainWindow.Children.Count;
            CurrentFloor = floornum;
            IsNoLogin = isNoLogin;

            LstEscapeRoutes = ObjEscapeRoutes.GetAll();
            LstCoordinate = ObjCoordinate.GetAll();
            LstPlanPartitionPointRecord = ObjPlanPartitionPointRecord.GetAll();
            LstEscapeRoutesCurrentFloor = lstEscapeRoutes;
            LstCoordinateCurrentFloor = lstCoordinate;
            LstPlanPartitionPointCurrent = lstPlanPartitionPointRecord;

            if (lstEscapeLines != null && lstEscapeRoutes != null)
            {
                EndPoint = lstEscapeRoutes.FindAll(x => x.EndPoint == 1);
                if (IsNoLogin)//未登录状态
                {
                    for (int i = 0; i < lstEscapeLines.Count; i++)
                    {
                        LstEscapeLinesCurrentFloor.Add(lstEscapeLines[i]);
                    }

                    for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                    {
                        LstEscapeLinesCurrentFloor[i].TransformX1 = LstEscapeLinesCurrentFloor[i].NLineX1;
                        LstEscapeLinesCurrentFloor[i].TransformX2 = LstEscapeLinesCurrentFloor[i].NLineX2;
                        LstEscapeLinesCurrentFloor[i].TransformY1 = LstEscapeLinesCurrentFloor[i].NLineY1;
                        LstEscapeLinesCurrentFloor[i].TransformY2 = LstEscapeLinesCurrentFloor[i].NLineY2;
                    }
                    for (int i = 0; i < EndPoint.Count; i++)
                    {
                        CoordinateInfo infoCoordinate = LstCoordinateCurrentFloor.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == EndPoint[i].ID);
                        Point point = new Point(infoCoordinate.NLOriginX, infoCoordinate.NLOriginY);
                        Ends.Add(point);
                    }
                }
                else
                {
                    for (int i = 0; i < lstEscapeLines.Count; i++)
                    {
                        LstEscapeLinesCurrentFloor.Add(lstEscapeLines[i]);
                    }
                    for (int i = 0; i < EndPoint.Count; i++)
                    {
                        CoordinateInfo infoCoordinate = LstCoordinateCurrentFloor.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == EndPoint[i].ID);
                        Point point = new Point(infoCoordinate.TransformX, infoCoordinate.TransformY);
                        Ends.Add(point);
                    }
                }
            }

            if (LstEscapeLinesCurrentFloor.Count > 0 && LstPlanPartitionPointCurrent.Count > 0 && LstEscapeRoutes.Count > 0)
            {
                //GetFootDropLine(LstPlanPartitionPointCurrent);
                FootDropLines = footDropLines;
                PrintRoute();
            }

            return PrintedLines;
        }

        /// <summary>
        /// 求出新增报警点对应的线段和逃生路线出发点
        /// </summary>
        /// <param name="fireAlarmPoint">新增的火灾报警点</param>
        /// <returns></returns>
        public List<AlarmPoint> GetFootDropLine(List<PlanPartitionPointRecordInfo> lstPlanPartitionPoint, List<EscapeRoutesInfo> LstEscapeRoutes, List<EscapeLinesInfo> LstEscapeLinesCurrent, List<CoordinateInfo> lstCoordinate, bool isNoLogin)
        {
            Point perpendicular = new Point();
            AlarmPoint NowFootDropLine = new AlarmPoint();

            for (int i = 0; i < lstPlanPartitionPoint.Count; i++)
            {
                Line footDropLine = new Line();
                CoordinateInfo infoCoordinate = lstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == lstPlanPartitionPoint[i].ID);
                if (lstPlanPartitionPoint[i].EscapeLineID != null && lstPlanPartitionPoint[i].EscapeLineID != -1)
                {
                    EscapeLinesInfo infoEscapeLine = LstEscapeLinesCurrent.Find(x => x.ID == lstPlanPartitionPoint[i].EscapeLineID);
                    if (infoEscapeLine is null) continue;
                    footDropLine.Name = infoEscapeLine.Name;
                    if (isNoLogin)
                    {
                        footDropLine.X1 = infoEscapeLine.NLineX1;
                        footDropLine.Y1 = infoEscapeLine.NLineY1;
                        footDropLine.X2 = infoEscapeLine.NLineX2;
                        footDropLine.Y2 = infoEscapeLine.NLineY2;
                    }
                    else
                    {
                        footDropLine.X1 = infoEscapeLine.TransformX1;
                        footDropLine.Y1 = infoEscapeLine.TransformY1;
                        footDropLine.X2 = infoEscapeLine.TransformX2;
                        footDropLine.Y2 = infoEscapeLine.TransformY2;
                    }
                }
                else
                {
                    footDropLine = GetLine(lstPlanPartitionPoint[i], LstEscapeRoutes, LstEscapeLinesCurrent, lstCoordinate, isNoLogin);
                }

                if (footDropLine != null)
                {
                    footDropLine.Tag = null;
                    //计算出火灾报警点到路线的垂足点
                    double k1;//line的斜率
                    double x, y;
                    if (footDropLine.X1 == footDropLine.X2)
                    {
                        //AddTurningPointImage(footDropLine.X1, fireAlarmPoint.Y, this.EscapeRoutes.Children.Count + 1);
                        if (isNoLogin)
                        {
                            perpendicular.X = Math.Round(footDropLine.X1);
                            perpendicular.Y = Math.Round(infoCoordinate.NLOriginY);
                        }
                        else
                        {
                            perpendicular.X = Math.Round(footDropLine.X1);
                            perpendicular.Y = Math.Round(infoCoordinate.TransformY);
                        }
                    }
                    else
                    {
                        k1 = (footDropLine.Y1 - footDropLine.Y2) / (footDropLine.X1 - footDropLine.X2);
                        if (k1 == 0)
                        {
                            if (isNoLogin)
                            {
                                if (infoCoordinate.NLOriginX <= (footDropLine.X1 < footDropLine.X2 ? footDropLine.X1 : footDropLine.X2))
                                {
                                    infoCoordinate.NLOriginX = footDropLine.X1;
                                }
                                if (infoCoordinate.NLOriginX >= (footDropLine.X1 > footDropLine.X2 ? footDropLine.X1 : footDropLine.X2))
                                {
                                    infoCoordinate.NLOriginX = footDropLine.X2;
                                }
                                perpendicular.X = Math.Round(infoCoordinate.NLOriginX);
                                perpendicular.Y = Math.Round(footDropLine.Y1);
                            }
                            else
                            {
                                if (infoCoordinate.TransformX <= (footDropLine.X1 < footDropLine.X2 ? footDropLine.X1 : footDropLine.X2))
                                {
                                    infoCoordinate.TransformX = footDropLine.X1;
                                }
                                if (infoCoordinate.TransformX >= (footDropLine.X1 > footDropLine.X2 ? footDropLine.X1 : footDropLine.X2))
                                {
                                    infoCoordinate.TransformX = footDropLine.X2;
                                }
                                perpendicular.X = Math.Round(infoCoordinate.TransformX);
                                perpendicular.Y = Math.Round(footDropLine.Y1);
                            }
                            //AddTurningPointImage(fireAlarmPoint.X, footDropLine.Y1, this.EscapeRoutes.Children.Count + 1);
                        }
                        else
                        {
                            if (isNoLogin)
                            {
                                x = (infoCoordinate.NLOriginY - footDropLine.Y1 + infoCoordinate.NLOriginX / k1 + footDropLine.X1 * k1) / (1 / k1 + k1);
                                perpendicular.X = Math.Round(x);
                                y = k1 * x - footDropLine.X1 * k1 + footDropLine.Y1;
                                perpendicular.Y = Math.Round(y);
                            }
                            else
                            {
                                x = (infoCoordinate.TransformY - footDropLine.Y1 + infoCoordinate.TransformX / k1 + footDropLine.X1 * k1) / (1 / k1 + k1);
                                perpendicular.X = Math.Round(x);
                                y = k1 * x - footDropLine.X1 * k1 + footDropLine.Y1;
                                perpendicular.Y = Math.Round(y);
                            }
                            //AddTurningPointImage(Math.Round(x), Math.Round(y), this.EscapeRoutes.Children.Count + 1);
                        }
                    }
                }
                else
                {
                    footDropLine = new Line();
                    double distance = 0;
                    CoordinateInfo infoCod = lstCoordinate.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == LstEscapeRoutes[0].ID);
                    double mindistance = Math.Sqrt((infoCod.TransformX - infoCoordinate.TransformX) * (infoCod.TransformX - infoCoordinate.TransformX) + (infoCod.TransformY - infoCoordinate.TransformY) * (infoCod.TransformY - infoCoordinate.TransformY));
                    perpendicular = new Point(infoCod.TransformX, infoCod.TransformY);
                    for (int j = 1; j < LstEscapeRoutes.Count; j++)
                    {
                        infoCod = lstCoordinate.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == LstEscapeRoutes[j].ID);
                        distance = Math.Sqrt((infoCod.TransformX - infoCoordinate.TransformX) * (infoCod.TransformX - infoCoordinate.TransformX) + (infoCod.TransformY - infoCoordinate.TransformY) * (infoCod.TransformY - infoCoordinate.TransformY));
                        if (distance < mindistance)
                        {
                            mindistance = distance;
                            perpendicular = new Point(infoCod.TransformX, infoCod.TransformY);
                        }
                    }
                    for (int j = 0; j < LstEscapeLinesCurrent.Count; j++)
                    {
                        if (LstEscapeLinesCurrent[j] == null)
                        {
                            continue;
                        }
                        else
                        {
                            if ((LstEscapeLinesCurrent[j].TransformX1 == perpendicular.X && LstEscapeLinesCurrent[j].TransformY1 == perpendicular.Y) || (LstEscapeLinesCurrent[j].TransformX2 == perpendicular.X && LstEscapeLinesCurrent[j].TransformY2 == perpendicular.Y))
                            {
                                footDropLine.X1 = LstEscapeLinesCurrent[j].TransformX1;
                                footDropLine.X2 = LstEscapeLinesCurrent[j].TransformX2;
                                footDropLine.Y1 = LstEscapeLinesCurrent[j].TransformY1;
                                footDropLine.Y2 = LstEscapeLinesCurrent[j].TransformY2;
                                break;
                            }
                        }
                    }
                }
                footDropLine.Tag = null;
                NowFootDropLine.Perpendicular = perpendicular;
                NowFootDropLine.FootDropLine = footDropLine;
                FootDropLines.Add(NowFootDropLine);
            }

            return FootDropLines;
        }

        public List<AlarmPoint> GetFootDropOriginLine(List<PlanPartitionPointRecordInfo> lstPlanPartitionPoint, List<EscapeRoutesInfo> LstEscapeRoutes, List<EscapeLinesInfo> LstEscapeLinesCurrent, List<CoordinateInfo> lstCoordinate)
        {
            Point perpendicular = new Point();
            AlarmPoint NowFootDropLine = new AlarmPoint();

            for (int i = 0; i < lstPlanPartitionPoint.Count; i++)
            {
                Line footDropLine = new Line();
                if (lstPlanPartitionPoint[i].EscapeLineID != null)
                {
                    EscapeLinesInfo infoEscapeLine = LstEscapeLinesCurrent.Find(x => x.ID == lstPlanPartitionPoint[i].EscapeLineID);
                    if (infoEscapeLine is null) continue;
                    footDropLine.Name = infoEscapeLine.Name;
                    footDropLine.X1 = infoEscapeLine.LineX1;
                    footDropLine.Y1 = infoEscapeLine.LineY1;
                    footDropLine.X2 = infoEscapeLine.LineX2;
                    footDropLine.Y2 = infoEscapeLine.LineY2;
                }

                if (footDropLine != null)
                {
                    footDropLine.Tag = null;
                    CoordinateInfo infoCod = lstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.ID == lstPlanPartitionPoint[i].ID);
                    //计算出火灾报警点到路线的垂足点
                    double k1;//line的斜率
                    double x, y;
                    if (footDropLine.X1 == footDropLine.X2)
                    {
                        //AddTurningPointImage(footDropLine.X1, fireAlarmPoint.Y, this.EscapeRoutes.Children.Count + 1);
                        if (infoCod.OriginY <= (footDropLine.Y1 < footDropLine.Y2 ? footDropLine.Y1 : footDropLine.Y2))
                        {
                            perpendicular.Y = footDropLine.Y1;
                        }
                        else
                        {
                            if (infoCod.OriginY >= (footDropLine.Y1 > footDropLine.Y2 ? footDropLine.Y1 : footDropLine.Y2))
                            {
                                perpendicular.Y = footDropLine.Y2;
                            }
                            else
                            {
                                perpendicular.Y = infoCod.OriginY;
                            }
                        }
                        perpendicular.X = Math.Round(footDropLine.X1);
                        perpendicular.Y = Math.Round(perpendicular.Y);
                    }
                    else
                    {
                        k1 = (footDropLine.Y1 - footDropLine.Y2) / (footDropLine.X1 - footDropLine.X2);
                        if (k1 == 0)
                        {
                            if (infoCod.OriginX <= (footDropLine.X1 < footDropLine.X2 ? footDropLine.X1 : footDropLine.X2))
                            {
                                perpendicular.X = footDropLine.X1;
                            }
                            else
                            {
                                if (infoCod.OriginX >= (footDropLine.X1 > footDropLine.X2 ? footDropLine.X1 : footDropLine.X2))
                                {
                                    perpendicular.X = footDropLine.X2;
                                }
                                else
                                {
                                    perpendicular.X = infoCod.OriginX;
                                }
                            }
                            perpendicular.X = Math.Round(perpendicular.X);
                            perpendicular.Y = Math.Round(footDropLine.Y1);
                            //AddTurningPointImage(fireAlarmPoint.X, footDropLine.Y1, this.EscapeRoutes.Children.Count + 1);
                        }
                        else
                        {
                            x = (infoCod.OriginY - footDropLine.Y1 + infoCod.OriginX / k1 + footDropLine.X1 * k1) / (1 / k1 + k1);
                            perpendicular.X = Math.Round(x);
                            y = k1 * x - footDropLine.X1 * k1 + footDropLine.Y1;
                            perpendicular.Y = Math.Round(y);
                            //AddTurningPointImage(Math.Round(x), Math.Round(y), this.EscapeRoutes.Children.Count + 1);
                        }
                    }
                }
                footDropLine.Tag = null;
                NowFootDropLine.Perpendicular = perpendicular;
                NowFootDropLine.FootDropLine = footDropLine;
                FootDropLines.Add(NowFootDropLine);
            }

            return FootDropLines;
        }

        /// <summary>
        /// 通过报警点找出距离最近的线段
        /// </summary>
        /// <param name="fireAlarmPoint">新增的报警点</param>
        /// <returns></returns>
        private Line GetLine(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord, List<EscapeRoutesInfo> LstEscapeRoutes, List<EscapeLinesInfo> LstEscapeLinesCurrent, List<CoordinateInfo> lstCoordinate, bool isNoLogin)
        {
            CoordinateInfo infoCoordinate = lstCoordinate.Find(z => z.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && z.TableID == infoPlanPartitionPointRecord.ID);
            List<EscapeLinesInfo> Nearline = new List<EscapeLinesInfo>();
            List<double> Distance = new List<double>();
            double distance;
            //计算出火灾报警点到路线的垂足点
            double k1;//line的斜率
            double x, y;
            Point Perpendicular = new Point();

            for (int i = 0; i < LstEscapeLinesCurrent.Count; i++)
            {
                //判断路线水平或垂直时报警点是否在线段范围内
                if ((LstEscapeLinesCurrent[i].LineX1 == LstEscapeLinesCurrent[i].LineX2 || LstEscapeLinesCurrent[i].LineY1 == LstEscapeLinesCurrent[i].LineY2) && ((infoCoordinate.OriginX >= (LstEscapeLinesCurrent[i].LineX1 < LstEscapeLinesCurrent[i].LineX2 ? LstEscapeLinesCurrent[i].LineX1 : LstEscapeLinesCurrent[i].LineX2) && infoCoordinate.OriginX <= (LstEscapeLinesCurrent[i].LineX1 > LstEscapeLinesCurrent[i].LineX2 ? LstEscapeLinesCurrent[i].LineX1 : LstEscapeLinesCurrent[i].LineX2)) || (infoCoordinate.OriginY >= (LstEscapeLinesCurrent[i].LineY1 < LstEscapeLinesCurrent[i].LineY2 ? LstEscapeLinesCurrent[i].LineY1 : LstEscapeLinesCurrent[i].LineY2) && infoCoordinate.OriginY <= (LstEscapeLinesCurrent[i].LineY1 > LstEscapeLinesCurrent[i].LineY2 ? LstEscapeLinesCurrent[i].LineY1 : LstEscapeLinesCurrent[i].LineY2))))
                {
                    Nearline.Add(LstEscapeLinesCurrent[i]);
                }
                //路线不是水平或垂直时，判断路线是否在线段范围内
                if (LstEscapeLinesCurrent[i].LineX1 != LstEscapeLinesCurrent[i].LineX2 && LstEscapeLinesCurrent[i].LineY1 != LstEscapeLinesCurrent[i].LineY2)
                {
                    k1 = (LstEscapeLinesCurrent[i].LineY1 - LstEscapeLinesCurrent[i].LineY2) / (LstEscapeLinesCurrent[i].LineX1 - LstEscapeLinesCurrent[i].LineX2);
                    x = (infoCoordinate.OriginY - LstEscapeLinesCurrent[i].LineY1 + infoCoordinate.OriginX / k1 + LstEscapeLinesCurrent[i].LineX1 * k1) / (1 / k1 + k1);
                    y = k1 * x - LstEscapeLinesCurrent[i].LineX1 * k1 + LstEscapeLinesCurrent[i].LineY1;
                    Perpendicular.X = Math.Round(x);
                    Perpendicular.Y = Math.Round(y);
                    if ((Perpendicular.X >= (LstEscapeLinesCurrent[i].LineX1 < LstEscapeLinesCurrent[i].LineX2 ? LstEscapeLinesCurrent[i].LineX1 : LstEscapeLinesCurrent[i].LineX2) && Perpendicular.X <= (LstEscapeLinesCurrent[i].LineX1 > LstEscapeLinesCurrent[i].LineX2 ? LstEscapeLinesCurrent[i].LineX1 : LstEscapeLinesCurrent[i].LineX2)) && (Perpendicular.Y >= (LstEscapeLinesCurrent[i].LineY1 < LstEscapeLinesCurrent[i].LineY2 ? LstEscapeLinesCurrent[i].LineY1 : LstEscapeLinesCurrent[i].LineY2) && Perpendicular.Y <= (LstEscapeLinesCurrent[i].LineY1 > LstEscapeLinesCurrent[i].LineY2 ? LstEscapeLinesCurrent[i].LineY1 : LstEscapeLinesCurrent[i].LineY2)))
                    {
                        Nearline.Add(LstEscapeLinesCurrent[i]);
                    }
                }
            }

            for (int i = 0; i < Nearline.Count; i++)
            {
                if (Nearline[i].LineX1 == Nearline[i].LineX2)
                {
                    distance = Math.Abs(infoCoordinate.OriginX - Nearline[i].LineX1);
                    Distance.Add(distance);
                }
                else
                {
                    k1 = (Nearline[i].LineY1 - Nearline[i].LineY2) / (Nearline[i].LineX1 - Nearline[i].LineX2);
                    if (k1 == 0)
                    {
                        distance = Math.Abs(infoCoordinate.OriginY - Nearline[i].LineY2);
                        Distance.Add(distance);
                    }
                    else
                    {
                        x = (infoCoordinate.OriginY - Nearline[i].LineY1 + infoCoordinate.OriginX / k1 + Nearline[i].LineX1 * k1) / (1 / k1 + k1);
                        y = k1 * x - Nearline[i].LineX1 * k1 + Nearline[i].LineY1;
                        Perpendicular.X = Math.Round(x);
                        Perpendicular.Y = Math.Round(y);

                        double square = (infoCoordinate.OriginX - Perpendicular.X) * (infoCoordinate.OriginX - Perpendicular.X) + (infoCoordinate.OriginY - Perpendicular.Y) * (infoCoordinate.OriginY - Perpendicular.Y);
                        distance = Math.Sqrt(square);
                        Distance.Add(distance);

                    }
                }
            }

            int minSubscript = 0;
            for (int i = 1; i < Distance.Count; i++)
            {
                if (Distance[i] < Distance[minSubscript])
                {
                    minSubscript = i;
                }
            }

            int minPoint = 0;
            CoordinateInfo infoCod = lstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.TableID == LstEscapeRoutes[0].ID);
            double Pointdistance = Math.Sqrt((infoCod.OriginX - infoCoordinate.OriginX) * (infoCod.OriginX - infoCoordinate.OriginX) + (infoCod.OriginY - infoCoordinate.OriginY) * (infoCod.OriginY - infoCoordinate.OriginY));//最短距离
            double PointDistance;
            for (int i = 1; i < LstEscapeRoutes.Count; i++)
            {
                infoCod = lstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.TableID == LstEscapeRoutes[i].ID);
                PointDistance = Math.Sqrt((infoCod.OriginX - infoCoordinate.OriginX) * (infoCod.OriginX - infoCoordinate.OriginX) + (infoCod.OriginY - infoCoordinate.OriginY) * (infoCod.OriginY - infoCoordinate.OriginY));
                if (PointDistance < Pointdistance)
                {
                    Pointdistance = PointDistance;
                    minPoint = i;
                }
            }



            if (Nearline.Count != 0)
            {
                if (Distance[minSubscript] < Pointdistance)
                {
                    Line line = new Line();
                    line.Name = Nearline[minSubscript].Name;
                    if (isNoLogin)
                    {
                        line.X1 = Nearline[minSubscript].NLineX1;
                        line.X2 = Nearline[minSubscript].NLineX2;
                        line.Y1 = Nearline[minSubscript].NLineY1;
                        line.Y2 = Nearline[minSubscript].NLineY2;
                    }
                    else
                    {
                        line.X1 = Nearline[minSubscript].TransformX1;
                        line.X2 = Nearline[minSubscript].TransformX2;
                        line.Y1 = Nearline[minSubscript].TransformY1;
                        line.Y2 = Nearline[minSubscript].TransformY2;
                    }
                    return line;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///火灾报警点对应的逃生路线生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintRoute()
        {
            for (int i = 0; i < FootDropLines.Count; i++)
            {
                if (i == 0)
                {
                    //新增报警点不是出口，或者新增报警点是其中一个出口并且存在其他出口时
                    if (!Ends.Contains(FootDropLines[i].Perpendicular) || (Ends.Contains(FootDropLines[i].Perpendicular) && Ends.Count > 1))
                    {
                        //bool LeftLine = false;
                        Point MinEndPoint = new Point();
                        Point MaxEndPoint = new Point();
                        if (FootDropLines[i].FootDropLine.X1 == FootDropLines[i].FootDropLine.X2)
                        {
                            if (FootDropLines[i].FootDropLine.Y1 < FootDropLines[i].FootDropLine.Y2)
                            {
                                MinEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                MinEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                MaxEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                            }
                            else
                            {
                                MinEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                MinEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                MaxEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                            }
                        }
                        else
                        {
                            if (FootDropLines[i].FootDropLine.X1 < FootDropLines[i].FootDropLine.X2)
                            {
                                MinEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                MinEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                MaxEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                            }
                            else
                            {
                                MinEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                MinEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                MaxEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                            }
                        }

                        if (PrintedLines.FindAll(x => x.X1 == FootDropLines[i].FootDropLine.X1 && x.X2 == FootDropLines[i].FootDropLine.X2 && x.Y1 == FootDropLines[i].FootDropLine.Y1 && x.Y2 == FootDropLines[i].FootDropLine.Y2).Count == 0)
                        {
                            PrintedLines.Add(FootDropLines[i].FootDropLine);
                        }
                        //新增线路坐标值小的一端

                        if(JudgeIsEnd(FootDropLines[i].FootDropLine,MinEndPoint))
                        {
                            SectionPrint(MinEndPoint, FootDropLines[i]);
                        }

                        //新增线路坐标值大的一端
                        if(JudgeIsEnd(FootDropLines[i].FootDropLine,MaxEndPoint))
                        {
                            SectionPrint(MaxEndPoint, FootDropLines[i]);
                        }

                        if (Ends.Contains(FootDropLines[i].Perpendicular) && Ends.Count > 1)
                        {
                            for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                            {
                                if (LstEscapeLinesCurrentFloor[j] == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    Line RemainingLine = new Line();
                                    RemainingLine.Name = LstEscapeLinesCurrentFloor[i].Name;
                                    RemainingLine.X1 = LstEscapeLinesCurrentFloor[i].TransformX1;
                                    RemainingLine.Y1 = LstEscapeLinesCurrentFloor[i].TransformY1;
                                    RemainingLine.X2 = LstEscapeLinesCurrentFloor[i].TransformX2;
                                    RemainingLine.Y2 = LstEscapeLinesCurrentFloor[i].TransformY2;
                                    if (PrintedLines.FindAll(x => x.X1 == RemainingLine.X1 && x.X2 == RemainingLine.X2 && x.Y1 == RemainingLine.Y1 && x.Y2 == RemainingLine.Y2).Count == 0)
                                    {
                                        Point NextPoint = new Point();
                                        if (RemainingLine.X1 == FootDropLines[i].Perpendicular.X && RemainingLine.Y1 == FootDropLines[i].Perpendicular.Y)
                                        {
                                            NextPoint.X = RemainingLine.X2;
                                            NextPoint.Y = RemainingLine.Y2;
                                            PrintedLines.Add(RemainingLine);
                                            AdjoinLine adjoin = new AdjoinLine();
                                            adjoin.alarm.FootDropLine = RemainingLine;
                                            adjoin.alarm.Perpendicular = FootDropLines[i].Perpendicular;
                                            UpdateLine(NextPoint, adjoin, false);
                                            RemainingLine.Tag = FootDropLines[i].Perpendicular;
                                            if (!Ends.Contains(NextPoint))
                                            {
                                                RouteAgain(RemainingLine, NextPoint);
                                            }
                                        }

                                        if (RemainingLine.X2 == FootDropLines[i].Perpendicular.X && RemainingLine.Y2 == FootDropLines[i].Perpendicular.Y)
                                        {
                                            NextPoint.X = RemainingLine.X1;
                                            NextPoint.Y = RemainingLine.Y1;

                                            PrintedLines.Add(RemainingLine);
                                            AdjoinLine adjoin = new AdjoinLine();
                                            adjoin.alarm.FootDropLine = RemainingLine;
                                            adjoin.alarm.Perpendicular = FootDropLines[i].Perpendicular;
                                            UpdateLine(NextPoint, adjoin, false);
                                            RemainingLine.Tag = FootDropLines[i].Perpendicular;
                                            if (!Ends.Contains(NextPoint))
                                            {
                                                RouteAgain(RemainingLine, NextPoint);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        FootDropLinesPrinted.Add(FootDropLines[i]);//记录已经生成逃生路线的报警点
                    }
                }
                #region
                else
                {
                    if (PrintedLines.Find(x => x.Name == FootDropLines[i].FootDropLine.Name) != null)
                    {
                        PrintedLines.Find(x => x.Name == FootDropLines[i].FootDropLine.Name).Tag = null;
                    }
                    List<Image> OldImages = new List<Image>();
                    List<AdjoinLine> AdjoinLines = new List<AdjoinLine>();//记录与新报警点对应的线段相邻的线段
                    AdjoinLine adjoinLine = new AdjoinLine();
                    List<Point> AcmePoints = new List<Point>();//同一条线路上存在多个报警点对应的起点时，存储两端最大最小的起点
                    List<Line> LeftOtherLines = new List<Line>();
                    List<Line> RightOtherLines = new List<Line>();


                    //找出与新报警点相邻线段相邻的线段
                    #region 找出与新报警点相邻线段相邻的旧报警线段
                    for (int j = 0; j < FootDropLinesPrinted.Count; j++)
                    {
                        if (((FootDropLines[i].FootDropLine.X1 == FootDropLinesPrinted[j].FootDropLine.X1 && FootDropLines[i].FootDropLine.Y1 == FootDropLinesPrinted[j].FootDropLine.Y1) || (FootDropLines[i].FootDropLine.X1 == FootDropLinesPrinted[j].FootDropLine.X2 && FootDropLines[i].FootDropLine.Y1 == FootDropLinesPrinted[j].FootDropLine.Y2)) && FootDropLinesPrinted[j].FootDropLine != FootDropLines[i].FootDropLine)
                        {
                            //左侧端点是出口时不用记录
                            Point LeftEndpoint = new Point();
                            LeftEndpoint.X = FootDropLines[i].FootDropLine.X1;
                            LeftEndpoint.Y = FootDropLines[i].FootDropLine.Y1;
                            if (!Ends.Contains(LeftEndpoint))
                            {
                                adjoinLine.AdjacentPoint.X = FootDropLines[i].FootDropLine.X1;
                                adjoinLine.AdjacentPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                adjoinLine.alarm = FootDropLinesPrinted[j];
                                AdjoinLines.Add(adjoinLine);
                            }
                        }
                        if (((FootDropLines[i].FootDropLine.X2 == FootDropLinesPrinted[j].FootDropLine.X1 && FootDropLines[i].FootDropLine.Y2 == FootDropLinesPrinted[j].FootDropLine.Y1) || (FootDropLines[i].FootDropLine.X2 == FootDropLinesPrinted[j].FootDropLine.X2 && FootDropLines[i].FootDropLine.Y2 == FootDropLinesPrinted[j].FootDropLine.Y2)) && FootDropLinesPrinted[j].FootDropLine != FootDropLines[i].FootDropLine)
                        {
                            //右侧端点是出口时不用记录
                            Point RightEndPoint = new Point();
                            RightEndPoint.X = FootDropLines[i].FootDropLine.X2;
                            RightEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                            if (!Ends.Contains(RightEndPoint))
                            {
                                adjoinLine.AdjacentPoint.X = FootDropLines[i].FootDropLine.X2;
                                adjoinLine.AdjacentPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                adjoinLine.alarm = FootDropLinesPrinted[j];
                                AdjoinLines.Add(adjoinLine);
                            }
                        }
                    }
                    #endregion


                    //绘画线路箭头

                    Point LeftPiont = new Point();
                    Point RightPiont = new Point();
                    LeftPiont.X = FootDropLines[i].FootDropLine.X1;
                    LeftPiont.Y = FootDropLines[i].FootDropLine.Y1;
                    RightPiont.X = FootDropLines[i].FootDropLine.X2;
                    RightPiont.Y = FootDropLines[i].FootDropLine.Y2;

                    bool IsExistArrow = false;
                    for (int j = 0; j < _context.cvsMainWindow.Children.Count; j++)
                    {
                        Image OldImage = _context.cvsMainWindow.Children[j] as Image;
                        if (OldImage == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (OldImage.Tag is Line && (OldImage.Tag as Line).X1 == FootDropLines[i].FootDropLine.X1 && (OldImage.Tag as Line).X2 == FootDropLines[i].FootDropLine.X2 && (OldImage.Tag as Line).Y1 == FootDropLines[i].FootDropLine.Y1 && (OldImage.Tag as Line).Y2 == FootDropLines[i].FootDropLine.Y2)//寻找在报警点对应线段上的箭头
                            {
                                IsExistArrow = true;
                                break;
                            }
                        }
                    }
                    //删除新增报警点对应线段上旧箭头
                    _context.DeleteOldImage(FootDropLines[i]);

                    //判断新增报警点对应的线段上是否存在箭头，存在则为线路可行并改变线路，不存在则为该线路不可行，不需要改变原来线路布局
                    if (IsExistArrow || Ends.Contains(LeftPiont) || Ends.Contains(RightPiont))
                    {
                        #region
                        if (FootDropLines.FindAll(x => (x.Perpendicular == LeftPiont) || (x.Perpendicular == RightPiont)).Count != 0)
                        {
                            //FootDropLines.Add(FootDropLines[i]);
                            Point PerPoint = new Point();
                            List<AlarmPoint> LeftPer = FootDropLines.FindAll(x => x.Perpendicular == LeftPiont);
                            List<AlarmPoint> RightPer = FootDropLines.FindAll(x => x.Perpendicular == RightPiont);

                            //判断新的火灾报警点和旧火灾报警点是否是同一线段上
                            if (FootDropLines.FindAll(x => x.FootDropLine == FootDropLines[i].FootDropLine).Count != 0)
                            {
                                //AcmePoints = GetAcme(alarmpoint.FootDropLine);
                                AdjoinLine adjoin = new AdjoinLine();
                                if (LeftPer.Count != 0 && RightPer.Count == 0)
                                {
                                    if (FootDropLines.FindAll(x => x.FootDropLine == FootDropLines[i].FootDropLine).Count != 0)
                                    {
                                        PerPoint = GetFurthestPoint(LeftPiont, FootDropLines[i]);
                                        adjoin.AdjacentPoint = PerPoint;
                                        adjoin.alarm = FootDropLines.Find(x => x.Perpendicular == PerPoint);
                                    }
                                    else
                                    {
                                        adjoin.AdjacentPoint = FootDropLines[i].Perpendicular;
                                        adjoin.alarm = FootDropLines[i];
                                    }
                                    UpdateLine(LeftPiont, adjoin, true);

                                }

                                if (RightPer.Count != 0 && LeftPer.Count == 0)
                                {
                                    if (FootDropLines.FindAll(x => x.FootDropLine == FootDropLines[i].FootDropLine).Count != 0)
                                    {
                                        PerPoint = GetFurthestPoint(RightPiont, FootDropLines[i]);
                                        adjoin.AdjacentPoint = PerPoint;
                                        adjoin.alarm = FootDropLines.Find(x => x.Perpendicular == PerPoint);
                                    }
                                    else
                                    {
                                        adjoin.AdjacentPoint = FootDropLines[i].Perpendicular;
                                        adjoin.alarm = FootDropLines[i];
                                    }
                                    UpdateLine(RightPiont, adjoin, true);
                                }
                            }

                        }
                        else
                        {
                            Point LeftEndPoint = new Point();
                            Point RightEndPoint = new Point();

                            LeftEndPoint.X = FootDropLines[i].FootDropLine.X1;
                            LeftEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                            RightEndPoint.X = FootDropLines[i].FootDropLine.X2;
                            RightEndPoint.Y = FootDropLines[i].FootDropLine.Y2;

                            //判断新的火灾报警点和旧火灾报警点是否是同一线段上
                            if (FootDropLines.FindAll(x => x.FootDropLine.X1 == FootDropLines[i].FootDropLine.X1 && x.FootDropLine.X2 == FootDropLines[i].FootDropLine.X2 && x.FootDropLine.Y1 == FootDropLines[i].FootDropLine.Y1 && x.FootDropLine.Y2 == FootDropLines[i].FootDropLine.Y2).Count > 1)
                            {
                                //FootDropLines.Add(FootDropLines[i]);

                                AcmePoints = GetAcme(FootDropLines[i].FootDropLine);

                                #region 两端坐标小的一侧绘画箭头

                                PrintPartArrow(FootDropLines[i], AcmePoints, LeftEndPoint);

                                #endregion

                                #region 两端坐标大的一侧绘画箭头

                                PrintPartArrow(FootDropLines[i], AcmePoints, RightEndPoint);

                                #endregion


                            }
                            else
                            {
                                //FootDropLines.Add(FootDropLines[i]);
                                if (AdjoinLines.Count != 0)
                                {
                                    if (!Ends.Contains(LeftEndPoint) && !Ends.Contains(RightEndPoint))
                                    {
                                        AdjacentLinesPrinting(AdjoinLines, FootDropLines[i]);
                                    }
                                    //新增的报警点对应的线段两端存在出口
                                    if (Ends.Contains(LeftEndPoint) || Ends.Contains(RightEndPoint))
                                    {
                                        if (Ends.Contains(LeftEndPoint))
                                        {
                                            adjoinLine.alarm = FootDropLines[i];
                                            adjoinLine.AdjacentPoint = LeftEndPoint;
                                            _context.DeleteOldImage(adjoinLine.alarm);
                                            UpdateLine(LeftEndPoint, adjoinLine, false);
                                        }

                                        if (Ends.Contains(RightEndPoint))
                                        {
                                            adjoinLine.alarm = FootDropLines[i];
                                            adjoinLine.AdjacentPoint = RightEndPoint;
                                            _context.DeleteOldImage(adjoinLine.alarm);
                                            UpdateLine(RightEndPoint, adjoinLine, false);
                                        }

                                        if (AdjoinLines.FindAll(x => Ends.Contains(x.AdjacentPoint)).Count != 0)
                                        {
                                            bool IsTwoLine = false;
                                            for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                                            {
                                                if (LstEscapeLinesCurrentFloor[j] == null)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    Line IsEndLine = new Line();
                                                    IsEndLine.Name = LstEscapeLinesCurrentFloor[j].Name;
                                                    IsEndLine.X1 = LstEscapeLinesCurrentFloor[j].TransformX1;
                                                    IsEndLine.X2 = LstEscapeLinesCurrentFloor[j].TransformX2;
                                                    IsEndLine.Y1 = LstEscapeLinesCurrentFloor[j].TransformY1;
                                                    IsEndLine.Y2 = LstEscapeLinesCurrentFloor[j].TransformY2;
                                                    LeftEndPoint.X = IsEndLine.X1;
                                                    LeftEndPoint.Y = IsEndLine.Y1;
                                                    RightEndPoint.X = IsEndLine.X2;
                                                    RightEndPoint.Y = IsEndLine.Y2;
                                                    if ((Ends.Contains(LeftEndPoint) || Ends.Contains(RightEndPoint)) && (IsEndLine.X1 != FootDropLines[i].FootDropLine.X1 || IsEndLine.X2 != FootDropLines[i].FootDropLine.X2 || IsEndLine.Y1 != FootDropLines[i].FootDropLine.Y1 || IsEndLine.Y2 != FootDropLines[i].FootDropLine.Y2) && AdjoinLines.FindAll(x => x.alarm.FootDropLine.X1 == IsEndLine.X1 && x.alarm.FootDropLine.X2 == IsEndLine.X2 && x.alarm.FootDropLine.Y1 == IsEndLine.Y1 && x.alarm.FootDropLine.Y2 == IsEndLine.Y2).Count == 0)
                                                    {
                                                        IsTwoLine = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            //以出口为交点的线都存在报警点对应的起点
                                            if (!IsTwoLine)
                                            {
                                                // _context.cvsMainWindow.Children.Clear();//清除图层上的所有元素
                                                _context.cvsMainWindow.Children.RemoveRange(ElementLoginCount, _context.cvsMainWindow.Children.Count - ElementLoginCount);//清除图层上的所有三角方向元素
                                                //PrintingFireAlarmPoint(FootDropLines, FireAlarmPoints);//绘画火灾报警点以及相应的逃生起点

                                                Point RelePoint = new Point();
                                                LeftEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                                LeftEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                                RightEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                                RightEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                                if (Ends.Contains(LeftEndPoint))
                                                {
                                                    RelePoint = LeftEndPoint;
                                                }
                                                if (Ends.Contains(RightEndPoint))
                                                {
                                                    RelePoint = RightEndPoint;
                                                }
                                                adjoinLine.AdjacentPoint = RelePoint;

                                                for (int j = 0; j < FootDropLines.Count; j++)
                                                {
                                                    adjoinLine.alarm = FootDropLines[j];
                                                    if ((FootDropLines[j].FootDropLine.X1 == RelePoint.X && FootDropLines[j].FootDropLine.Y1 == RelePoint.Y) || (FootDropLines[j].FootDropLine.X2 == RelePoint.X && FootDropLines[j].FootDropLine.Y2 == RelePoint.Y))
                                                    {
                                                        List<AlarmPoint> SameLine = FootDropLines.FindAll(x => x.FootDropLine == FootDropLines[j].FootDropLine);
                                                        if (SameLine.Count > 1)//检查在同一条线段上是否存在多个报警点
                                                        {
                                                            double minDistance = Math.Sqrt((SameLine[0].Perpendicular.X - RelePoint.X) * (SameLine[0].Perpendicular.X - RelePoint.X) + (SameLine[0].Perpendicular.Y - RelePoint.Y) * (SameLine[0].Perpendicular.Y - RelePoint.Y));
                                                            int minIndex = 0;
                                                            for (int h = 1; h < SameLine.Count; h++)
                                                            {
                                                                if (minDistance > Math.Sqrt((SameLine[h].Perpendicular.X - RelePoint.X) * (SameLine[h].Perpendicular.X - RelePoint.X) + (SameLine[h].Perpendicular.Y - RelePoint.Y) * (SameLine[h].Perpendicular.Y - RelePoint.Y)))
                                                                {
                                                                    minIndex = h;
                                                                    minDistance = Math.Sqrt((SameLine[h].Perpendicular.X - RelePoint.X) * (SameLine[h].Perpendicular.X - RelePoint.X) + (SameLine[h].Perpendicular.Y - RelePoint.Y) * (SameLine[h].Perpendicular.Y - RelePoint.Y));
                                                                }
                                                            }
                                                            adjoinLine.alarm = SameLine[minIndex];
                                                            _context.DeleteOldImage(adjoinLine.alarm);
                                                            UpdateLine(RelePoint, adjoinLine, false);
                                                        }
                                                        else
                                                        {
                                                            _context.DeleteOldImage(adjoinLine.alarm);
                                                            UpdateLine(RelePoint, adjoinLine, false);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                List<Line> NoEndLines = new List<Line>();
                                                //不是出口的那一端，存在旧报警点对应的线段，并且不存在其他想通的线段时
                                                for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                                                {
                                                    if (LstEscapeLinesCurrentFloor[j] == null)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        Line line = new Line();
                                                        line.Name = LstEscapeLinesCurrentFloor[j].Name;
                                                        line.X1 = LstEscapeLinesCurrentFloor[j].TransformX1;
                                                        line.X2 = LstEscapeLinesCurrentFloor[j].TransformX2;
                                                        line.Y1 = LstEscapeLinesCurrentFloor[j].TransformY1;
                                                        line.Y2 = LstEscapeLinesCurrentFloor[j].TransformY2;
                                                        if (((line.X1 == AdjoinLines[0].AdjacentPoint.X && line.Y1 == AdjoinLines[0].AdjacentPoint.Y) || (line.X2 == AdjoinLines[0].AdjacentPoint.X && line.Y2 == AdjoinLines[0].AdjacentPoint.Y)) && (line.X1 != FootDropLines[i].FootDropLine.X1 || line.X2 != FootDropLines[i].FootDropLine.X2 || line.Y1 != FootDropLines[i].FootDropLine.Y1 || line.Y2 != FootDropLines[i].FootDropLine.Y2) && AdjoinLines.FindAll(x => x.alarm.FootDropLine.X1 == line.X1 && x.alarm.FootDropLine.X2 == line.X2 && x.alarm.FootDropLine.Y1 == line.Y1 && x.alarm.FootDropLine.Y2 == line.Y2).Count == 0)
                                                        {
                                                            NoEndLines.Add(line);
                                                        }
                                                    }
                                                }

                                                if (NoEndLines.Count == 0)
                                                {
                                                    _context.DeleteOldImage(FootDropLines[i]);
                                                }
                                                else
                                                {
                                                    Point MinEndPoint = new Point();
                                                    Point MaxEndPoint = new Point();
                                                    if (FootDropLines[i].FootDropLine.X1 == FootDropLines[i].FootDropLine.X2)
                                                    {
                                                        if (FootDropLines[i].FootDropLine.Y1 < FootDropLines[i].FootDropLine.Y2)
                                                        {
                                                            MinEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                                            MinEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                                            MaxEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                                            MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                                        }
                                                        else
                                                        {
                                                            MinEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                                            MinEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                                            MaxEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                                            MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (FootDropLines[i].FootDropLine.X1 < FootDropLines[i].FootDropLine.X2)
                                                        {
                                                            MinEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                                            MinEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                                            MaxEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                                            MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                                        }
                                                        else
                                                        {
                                                            MinEndPoint.X = FootDropLines[i].FootDropLine.X2;
                                                            MinEndPoint.Y = FootDropLines[i].FootDropLine.Y2;
                                                            MaxEndPoint.X = FootDropLines[i].FootDropLine.X1;
                                                            MaxEndPoint.Y = FootDropLines[i].FootDropLine.Y1;
                                                        }
                                                    }

                                                    if (PrintedLines.FindAll(x => x.X1 == FootDropLines[i].FootDropLine.X1 && x.X2 == FootDropLines[i].FootDropLine.X2 && x.Y1 == FootDropLines[i].FootDropLine.Y1 && x.Y2 == FootDropLines[i].FootDropLine.Y2).Count == 0)
                                                    {
                                                        PrintedLines.Add(FootDropLines[i].FootDropLine);
                                                    }
                                                    //新增线路坐标值小的一端

                                                    SectionPrint(MinEndPoint, FootDropLines[i]);

                                                    //新增线路坐标值大的一端

                                                    SectionPrint(MaxEndPoint, FootDropLines[i]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            List<Line> NoAlarmLines = new List<Line>();//存放相邻的但不是报警点对应的线段
                                            for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                                            {
                                                if (LstEscapeLinesCurrentFloor[j] == null)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    Line line = new Line();
                                                    line.Name = LstEscapeLinesCurrentFloor[j].Name;
                                                    line.X1 = LstEscapeLinesCurrentFloor[j].TransformX1;
                                                    line.X2 = LstEscapeLinesCurrentFloor[j].TransformX2;
                                                    line.Y1 = LstEscapeLinesCurrentFloor[j].TransformY1;
                                                    line.Y2 = LstEscapeLinesCurrentFloor[j].TransformY2;
                                                    if (PrintedLines.Find(x => x.Name == line.Name) != null)
                                                    {
                                                        line.Tag = PrintedLines.Find(x => x.Name == line.Name).Tag;
                                                    }
                                                    if (FootDropLines.FindAll(x => x.FootDropLine.X1 == line.X1 && x.FootDropLine.X2 == line.X2 && x.FootDropLine.Y1 == line.Y1 && x.FootDropLine.Y2 == line.Y2).Count == 0 && ((line.X1 == AdjoinLines[0].AdjacentPoint.X && line.Y1 == AdjoinLines[0].AdjacentPoint.Y) || (line.X2 == AdjoinLines[0].AdjacentPoint.X && line.Y2 == AdjoinLines[0].AdjacentPoint.Y)))
                                                    {
                                                        NoAlarmLines.Add(line);
                                                    }
                                                }
                                            }

                                            if (NoAlarmLines.Count != 0)
                                            {
                                                if (NoAlarmLines.FindAll(x => (Point)x.Tag == AdjoinLines[0].AdjacentPoint).Count == 0)
                                                {
                                                    bool IsExist = false;
                                                    int index = NoAlarmLines.Count;
                                                    for (int j = 0; j < NoAlarmLines.Count; j++)
                                                    {
                                                        Point finPoint = (Point)NoAlarmLines[j].Tag;
                                                        ChaRouteDirection.Clear();
                                                        IsExist = JudgeDirection(NoAlarmLines[j], finPoint);
                                                        if (IsExist)
                                                        {
                                                            index = j;
                                                            break;
                                                        }
                                                    }
                                                    if (index != NoAlarmLines.Count)
                                                    {
                                                        AdjacentLinesPrinting(AdjoinLines, FootDropLines[i]);
                                                        //RouteAgain(NoAlarmLines[index], (Point)NoAlarmLines[index].Tag);
                                                    }
                                                    else
                                                    {
                                                        DeleteArrow(FootDropLines[i].FootDropLine, AdjoinLines[0].AdjacentPoint);
                                                    }
                                                }
                                                else
                                                {
                                                    ChaRouteDirection.Clear();
                                                    if (JudgeDirection(FootDropLines[i].FootDropLine, AdjoinLines[0].AdjacentPoint))
                                                    {
                                                        adjoinLine.alarm = FootDropLines[i];
                                                        adjoinLine.AdjacentPoint = AdjoinLines[0].AdjacentPoint;
                                                        //DeleteOldImage(adjoinLine.alarm.FootDropLine);
                                                        UpdateLine(AdjoinLines[0].AdjacentPoint, adjoinLine, false);
                                                    }
                                                    else
                                                    {
                                                        AlreadyDeleLine.Clear();
                                                        DeleteArrow(FootDropLines[i].FootDropLine, AdjoinLines[0].AdjacentPoint);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                for (int j = 0; j < AdjoinLines.Count; j++)
                                                {
                                                    _context.DeleteOldImage(AdjoinLines[j].alarm, AdjoinLines[j].AdjacentPoint);
                                                }
                                            }
                                        }
                                        //}
                                    }
                                }
                                else
                                {
                                    #region 新增报警点对应线段的左侧部分
                                    Point NextOrigin = new Point();
                                    NextOrigin.X = FootDropLines[i].FootDropLine.X1;
                                    NextOrigin.Y = FootDropLines[i].FootDropLine.Y1;
                                    if (Ends.Contains(NextOrigin))
                                    {
                                        adjoinLine.alarm = FootDropLines[i];
                                        adjoinLine.AdjacentPoint = NextOrigin;
                                        UpdateLine(NextOrigin, adjoinLine, false);
                                    }
                                    else
                                    {
                                        ChaRouteDirection.Clear();
                                        if (JudgeDirection(FootDropLines[i].FootDropLine, NextOrigin))
                                        {
                                            adjoinLine.alarm = FootDropLines[i];
                                            adjoinLine.AdjacentPoint = NextOrigin;
                                            UpdateLine(NextOrigin, adjoinLine, false);
                                            RouteAgain(adjoinLine.alarm.FootDropLine, NextOrigin);
                                        }
                                        else
                                        {
                                            AlreadyDeleLine.Clear();
                                            DeleteArrow(FootDropLines[i].FootDropLine, NextOrigin);
                                        }
                                    }
                                    #endregion

                                    #region 新增报警点对应线段的右侧部分
                                    NextOrigin.X = FootDropLines[i].FootDropLine.X2;
                                    NextOrigin.Y = FootDropLines[i].FootDropLine.Y2;
                                    if (Ends.Contains(NextOrigin))
                                    {
                                        adjoinLine.alarm = FootDropLines[i];
                                        adjoinLine.AdjacentPoint = NextOrigin;
                                        UpdateLine(NextOrigin, adjoinLine, false);
                                        PrintedLines.Add(adjoinLine.alarm.FootDropLine);
                                    }
                                    else
                                    {
                                        ChaRouteDirection.Clear();
                                        if (JudgeDirection(FootDropLines[i].FootDropLine, NextOrigin))
                                        {
                                            adjoinLine.alarm = FootDropLines[i];
                                            adjoinLine.AdjacentPoint = NextOrigin;
                                            UpdateLine(NextOrigin, adjoinLine, false);
                                            PrintedLines.Add(adjoinLine.alarm.FootDropLine);
                                            RouteAgain(adjoinLine.alarm.FootDropLine, NextOrigin);
                                        }
                                        else
                                        {
                                            AlreadyDeleLine.Clear();
                                            DeleteArrow(FootDropLines[i].FootDropLine, NextOrigin);
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion

                        if (Ends.Contains(FootDropLines[i].Perpendicular))
                        {
                            if (Ends.Count < 2)
                            {
                                //this.EscapeRoutesPoint.Children.Clear();
                                _context.cvsMainWindow.Children.RemoveRange(ElementLoginCount, _context.cvsMainWindow.Children.Count - ElementLoginCount);
                                //PrintingFireAlarmPoint(FootDropLines, FireAlarmPoints);//绘画火灾报警点以及相应的逃生起点
                            }
                            else
                            {
                                List<Line> AppointLine = new List<Line>();
                                for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                                {
                                    if (LstEscapeLinesCurrentFloor[j] == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        Line line = new Line();
                                        line.Name = LstEscapeLinesCurrentFloor[j].Name;
                                        line.X1 = LstEscapeLinesCurrentFloor[j].TransformX1;
                                        line.X2 = LstEscapeLinesCurrentFloor[j].TransformX2;
                                        line.Y1 = LstEscapeLinesCurrentFloor[j].TransformY1;
                                        line.Y2 = LstEscapeLinesCurrentFloor[j].TransformY2;
                                        if ((line.X1 != FootDropLines[i].FootDropLine.X1 || line.X2 != FootDropLines[i].FootDropLine.X2 || line.Y1 != FootDropLines[i].FootDropLine.Y1 || line.Y2 != FootDropLines[i].FootDropLine.Y2) && ((line.X1 == FootDropLines[i].Perpendicular.X && line.Y1 == FootDropLines[i].Perpendicular.Y) || (line.X2 == FootDropLines[i].Perpendicular.X && line.Y2 == FootDropLines[i].Perpendicular.Y)))
                                        {
                                            AppointLine.Add(line);
                                        }
                                    }
                                }

                                if (AppointLine.Count != 0)
                                {
                                    for (int j = 0; j < AppointLine.Count; j++)
                                    {
                                        AlarmPoint alarm = new AlarmPoint();
                                        alarm.FootDropLine = AppointLine[j];
                                        _context.DeleteOldImage(alarm);

                                        Point NextStartPoint = GetOtherPoint(AppointLine[j], FootDropLines[j].Perpendicular);

                                        List<AlarmPoint> dropLines = FootDropLines.FindAll(x => x.FootDropLine == AppointLine[j]);
                                        //线段的另一端不是火灾报警点的时候，判断画箭头；是报警点的时候删除该线段上的所有箭头
                                        if (FootDropLines.FindAll(x => x.Perpendicular == NextStartPoint).Count == 0)
                                        {
                                            AdjoinLine adjoin = new AdjoinLine();
                                            PrintedLines.Add(AppointLine[j]);
                                            if (dropLines.Count != 0)
                                            {
                                                double maxDistance = Math.Sqrt((FootDropLines[i].Perpendicular.X - dropLines[0].Perpendicular.X) * (FootDropLines[i].Perpendicular.X - dropLines[0].Perpendicular.X) + (FootDropLines[i].Perpendicular.Y - dropLines[0].Perpendicular.Y) * (FootDropLines[i].Perpendicular.Y - dropLines[0].Perpendicular.Y));
                                                Point maxPoint = dropLines[0].Perpendicular;

                                                for (int h = 1; h < dropLines.Count; h++)
                                                {
                                                    if (maxDistance < Math.Sqrt((FootDropLines[i].Perpendicular.X - dropLines[j].Perpendicular.X) * (FootDropLines[i].Perpendicular.X - dropLines[j].Perpendicular.X) + (FootDropLines[i].Perpendicular.Y - dropLines[j].Perpendicular.Y) * (FootDropLines[i].Perpendicular.Y - dropLines[j].Perpendicular.Y)))
                                                    {
                                                        maxDistance = Math.Sqrt((FootDropLines[i].Perpendicular.X - dropLines[j].Perpendicular.X) * (FootDropLines[i].Perpendicular.X - dropLines[j].Perpendicular.X) + (FootDropLines[i].Perpendicular.Y - dropLines[j].Perpendicular.Y) * (FootDropLines[i].Perpendicular.Y - dropLines[j].Perpendicular.Y));
                                                        maxPoint = dropLines[j].Perpendicular;
                                                    }
                                                }
                                                adjoin.alarm.FootDropLine = AppointLine[j];
                                                adjoin.alarm.Perpendicular = maxPoint;

                                            }
                                            else
                                            {
                                                adjoin.alarm.FootDropLine = AppointLine[j];
                                                adjoin.alarm.Perpendicular = FootDropLines[i].Perpendicular;
                                            }
                                            UpdateLine(NextStartPoint, adjoin, false);

                                            //bool IsExist = false;
                                            List<Line> nextlines = new List<Line>();
                                            for (int h = 0; h < LstEscapeLinesCurrentFloor.Count; h++)
                                            {
                                                if (LstEscapeLinesCurrentFloor[h] == null)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    Line nextline = new Line();
                                                    nextline.Name = LstEscapeLinesCurrentFloor[h].Name;
                                                    nextline.X1 = LstEscapeLinesCurrentFloor[h].TransformX1;
                                                    nextline.X2 = LstEscapeLinesCurrentFloor[h].TransformX2;
                                                    nextline.Y1 = LstEscapeLinesCurrentFloor[h].TransformY1;
                                                    nextline.Y2 = LstEscapeLinesCurrentFloor[h].TransformY2;
                                                    if ((nextline.X1 != AppointLine[j].X1 || nextline.X2 != AppointLine[j].X2 || nextline.Y1 != AppointLine[j].Y1 || nextline.Y2 != AppointLine[j].Y2) && FootDropLines.FindAll(x => x.FootDropLine.X1 == nextline.X1 && x.FootDropLine.X2 == nextline.X2 && x.FootDropLine.Y1 == nextline.Y1 && x.FootDropLine.Y2 == nextline.Y2).Count == 0 && ((nextline.X1 == NextStartPoint.X && nextline.Y1 == NextStartPoint.Y) || (nextline.X2 == NextStartPoint.X && nextline.Y2 == NextStartPoint.Y)))
                                                    {
                                                        nextlines.Add(nextline);
                                                    }
                                                }
                                            }

                                            if (nextlines.FindAll(x => x.Tag == null).Count == nextlines.Count)
                                            {
                                                JudgeEnd(AppointLine[j], GetOtherPoint(AppointLine[j], NextStartPoint));
                                            }
                                            else
                                            {
                                                if (JudgeDirection(AppointLine[j], NextStartPoint))
                                                {
                                                    RouteAgain(FootDropLines[i].FootDropLine, NextStartPoint);
                                                }
                                                else
                                                {
                                                    DeleteArrow(FootDropLines[i].FootDropLine, NextStartPoint);
                                                }
                                            }
                                        }
                                        //}
                                    }
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                }
                #endregion
            }
        }

        private void SectionPrint(Point MinORMax, AlarmPoint alarm)
        {
            bool IsExistLine = false;
            double interval = 0.0;
            double RotationAngle = 0.0;

            if (!Ends.Contains(MinORMax))
            {
                for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                {
                    if (LstEscapeLinesCurrentFloor[i] == null)
                    {
                        continue;
                    }
                    else
                    {
                        if ((LstEscapeLinesCurrentFloor[i].TransformX1 != alarm.FootDropLine.X1 || LstEscapeLinesCurrentFloor[i].TransformX2 != alarm.FootDropLine.X2 || LstEscapeLinesCurrentFloor[i].TransformY1 != alarm.FootDropLine.Y1 || LstEscapeLinesCurrentFloor[i].TransformY2 != alarm.FootDropLine.Y2) && ((LstEscapeLinesCurrentFloor[i].TransformX1 == MinORMax.X && LstEscapeLinesCurrentFloor[i].TransformY1 == MinORMax.Y) || (LstEscapeLinesCurrentFloor[i].TransformX2 == MinORMax.X && LstEscapeLinesCurrentFloor[i].TransformY2 == MinORMax.Y)))
                        {
                            IsExistLine = true;
                            break;
                        }
                    }
                }

                if (IsExistLine)
                {
                    #region 根据报警点线段计算每个箭头的间隔以及箭头旋转角度
                    if (alarm.FootDropLine.X1 == alarm.FootDropLine.X2)
                    {
                        if (MinORMax.Y < alarm.Perpendicular.Y)
                        {
                            interval = -1 * Pointdistance;
                            RotationAngle = -90;
                        }
                        else
                        {
                            interval = Pointdistance;
                            RotationAngle = 90;
                        }
                    }
                    else
                    {
                        if (alarm.FootDropLine.Y1 == alarm.FootDropLine.Y2)
                        {
                            if (MinORMax.X < alarm.Perpendicular.X)
                            {
                                interval = -1 * Pointdistance;
                                RotationAngle = -180;
                            }
                            else
                            {
                                interval = Pointdistance;
                                RotationAngle = 0;
                            }
                        }
                        else
                        {
                            double angle = Math.Atan((alarm.FootDropLine.Y2 - alarm.FootDropLine.Y1) / (alarm.FootDropLine.X2 - alarm.FootDropLine.X1)) * 180 / Math.PI;
                            if (MinORMax.X < alarm.Perpendicular.X)
                            {
                                interval = -1 * Pointdistance * Math.Cos(angle * Math.PI / 180);
                                RotationAngle = -180 + angle;
                            }
                            else
                            {
                                interval = Pointdistance * Math.Cos(angle * Math.PI / 180);
                                RotationAngle = angle;
                            }
                        }
                    }
                    #endregion

                    if (alarm.FootDropLine.X1 == alarm.FootDropLine.X2)
                    {
                        NextPrint(alarm.FootDropLine, alarm.Perpendicular.Y, MinORMax.Y, interval);
                        for (int i = (int)alarm.Perpendicular.Y + (int)interval; interval > 0 ? (i < MinORMax.Y) : (i > MinORMax.Y); i += (int)interval)
                        {
                            _context.PrintArrow(alarm.FootDropLine, alarm.FootDropLine.X1, i, RotationAngle);

                            NextPrint(alarm.FootDropLine, i, MinORMax.Y, interval);
                        }
                    }
                    else
                    {
                        NextPrint(alarm.FootDropLine, alarm.Perpendicular.X, MinORMax.X, interval);
                        for (int i = (int)alarm.Perpendicular.X + (int)interval; interval > 0 ? (i < MinORMax.X) : (i > MinORMax.X); i += (int)interval)
                        {
                            _context.PrintArrow(alarm.FootDropLine, i, alarm.FootDropLine.X1 == alarm.FootDropLine.X2 ? alarm.FootDropLine.Y1 - 10 : (i - alarm.FootDropLine.X1) * ((alarm.FootDropLine.Y2 - alarm.FootDropLine.Y1) / (alarm.FootDropLine.X2 - alarm.FootDropLine.X1)) + alarm.FootDropLine.Y1, RotationAngle);

                            NextPrint(alarm.FootDropLine, i, MinORMax.X, interval);
                        }
                    }
                }
            }
            else
            {
                #region 根据报警点线段计算每个箭头的间隔以及箭头旋转角度
                if (alarm.FootDropLine.X1 == alarm.FootDropLine.X2)
                {
                    if (MinORMax.Y < alarm.Perpendicular.Y)
                    {
                        interval = -1 * Pointdistance;
                        RotationAngle = -90;
                    }
                    else
                    {
                        interval = Pointdistance;
                        RotationAngle = 90;
                    }
                }
                else
                {
                    if (alarm.FootDropLine.Y1 == alarm.FootDropLine.Y2)
                    {
                        if (MinORMax.X < alarm.Perpendicular.X)
                        {
                            interval = -1 * Pointdistance;
                            RotationAngle = -180;
                        }
                        else
                        {
                            interval = Pointdistance;
                            RotationAngle = 0;
                        }
                    }
                    else
                    {
                        double angle = Math.Atan((alarm.FootDropLine.Y2 - alarm.FootDropLine.Y1) / (alarm.FootDropLine.X2 - alarm.FootDropLine.X1)) * 180 / Math.PI;
                        if (MinORMax.X < alarm.Perpendicular.X)
                        {
                            interval = -1 * Pointdistance * Math.Cos(angle * Math.PI / 180);
                            RotationAngle = -180 + angle;
                        }
                        else
                        {
                            interval = Pointdistance * Math.Cos(angle * Math.PI / 180);
                            RotationAngle = angle;
                        }
                    }
                }
                #endregion

                if (alarm.FootDropLine.X1 == alarm.FootDropLine.X2)
                {
                    NextPrint(alarm.FootDropLine, alarm.Perpendicular.Y, MinORMax.Y, interval);
                    for (int i = (int)alarm.Perpendicular.Y + (int)interval; interval > 0 ? (i < MinORMax.Y) : (i > MinORMax.Y); i += (int)interval)
                    {
                        _context.PrintArrow(alarm.FootDropLine, alarm.FootDropLine.X1, i, RotationAngle);

                        NextPrint(alarm.FootDropLine, i, MinORMax.Y, interval);
                    }
                }
                else
                {
                    NextPrint(alarm.FootDropLine, alarm.Perpendicular.X, MinORMax.X, interval);
                    for (int i = (int)alarm.Perpendicular.X + (int)interval; interval > 0 ? (i < MinORMax.X) : (i > MinORMax.X); i += (int)interval)
                    {
                        _context.PrintArrow(alarm.FootDropLine, i, alarm.FootDropLine.X1 == alarm.FootDropLine.X2 ? alarm.FootDropLine.Y1 - 10 : (i - alarm.FootDropLine.X1) * ((alarm.FootDropLine.Y2 - alarm.FootDropLine.Y1) / (alarm.FootDropLine.X2 - alarm.FootDropLine.X1)) + alarm.FootDropLine.Y1, RotationAngle);

                        NextPrint(alarm.FootDropLine, i, MinORMax.X, interval);
                    }
                }
            }
        }

        /// <summary>
        /// 根据新增报警点绘画对应线段的箭头
        /// </summary>
        /// <param name="point">新增报警点线段的两端其中一个坐标</param>
        /// <param name="adjoinLine">新增报警点以及相关线段的信息</param>
        /// <param name="IsEndPoint">是否反向绘画箭头</param>
        private void UpdateLine(Point point, AdjoinLine adjoinLine, bool IsEndPoint)
        {
            if ((adjoinLine.alarm.Perpendicular.X == adjoinLine.alarm.FootDropLine.X1 && adjoinLine.alarm.Perpendicular.Y == adjoinLine.alarm.FootDropLine.Y1) || (adjoinLine.alarm.Perpendicular.X == adjoinLine.alarm.FootDropLine.X2 && adjoinLine.alarm.Perpendicular.Y == adjoinLine.alarm.FootDropLine.Y2))
            {
                if (FootDropLines.FindAll(x => x.FootDropLine == adjoinLine.alarm.FootDropLine).Count == 0)
                {
                    Route(adjoinLine.alarm.FootDropLine, adjoinLine.alarm.Perpendicular);
                }
                else
                {
                    Point LeftPoint = new Point();
                    LeftPoint.X = adjoinLine.alarm.FootDropLine.X1;
                    LeftPoint.Y = adjoinLine.alarm.FootDropLine.Y1;
                    Point RightPoint = new Point();
                    RightPoint.X = adjoinLine.alarm.FootDropLine.X2;
                    RightPoint.Y = adjoinLine.alarm.FootDropLine.Y2;
                    if (Math.Sqrt((adjoinLine.alarm.Perpendicular.X - LeftPoint.X) * (adjoinLine.alarm.Perpendicular.X - LeftPoint.X) + (adjoinLine.alarm.Perpendicular.Y - LeftPoint.Y) * (adjoinLine.alarm.Perpendicular.Y - LeftPoint.Y)) < Math.Sqrt((adjoinLine.alarm.Perpendicular.X - RightPoint.X) * (adjoinLine.alarm.Perpendicular.X - RightPoint.X) + (adjoinLine.alarm.Perpendicular.Y - RightPoint.Y) * (adjoinLine.alarm.Perpendicular.Y - RightPoint.Y)))
                    {
                        if (JudgeDirection(adjoinLine.alarm.FootDropLine, LeftPoint))
                        {
                            RouteAgain(adjoinLine.alarm.FootDropLine, LeftPoint);
                        }
                    }
                    else
                    {
                        if (JudgeDirection(adjoinLine.alarm.FootDropLine, RightPoint))
                        {
                            RouteAgain(adjoinLine.alarm.FootDropLine, RightPoint);
                        }
                    }
                }
            }
            else
            {
                if (!IsEndPoint)
                {
                    if (adjoinLine.alarm.FootDropLine.X1 == adjoinLine.alarm.FootDropLine.X2)
                    {
                        if (point.Y <= adjoinLine.alarm.Perpendicular.Y)
                        {
                            for (int j = (int)adjoinLine.alarm.Perpendicular.Y - Pointdistance; j >= (adjoinLine.alarm.FootDropLine.Y1 < adjoinLine.alarm.FootDropLine.Y2 ? adjoinLine.alarm.FootDropLine.Y1 : adjoinLine.alarm.FootDropLine.Y2); j -= Pointdistance)
                            {
                                _context.PrintArrow(adjoinLine.alarm.FootDropLine, adjoinLine.alarm.FootDropLine.X1, j, -90);
                            }
                        }
                        else
                        {
                            for (int j = (int)adjoinLine.alarm.Perpendicular.Y + Pointdistance; j <= (adjoinLine.alarm.FootDropLine.Y1 > adjoinLine.alarm.FootDropLine.Y2 ? adjoinLine.alarm.FootDropLine.Y1 : adjoinLine.alarm.FootDropLine.Y2); j += Pointdistance)
                            {
                                _context.PrintArrow(adjoinLine.alarm.FootDropLine, adjoinLine.alarm.FootDropLine.X1, j, 90);
                            }
                        }
                    }
                    else
                    {
                        if (point.X <= adjoinLine.alarm.Perpendicular.X)
                        {
                            if (adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2)
                            {
                                for (int j = (int)adjoinLine.alarm.Perpendicular.X - Pointdistance; j >= (adjoinLine.alarm.FootDropLine.X1 < adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j -= Pointdistance)
                                {
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, adjoinLine.alarm.FootDropLine.Y1, -180);
                                }
                            }
                            if (!(adjoinLine.alarm.FootDropLine.X1 == adjoinLine.alarm.FootDropLine.X2) && !(adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2))
                            {
                                double angle = Math.Atan((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) * 180 / Math.PI;
                                double interval = Pointdistance * Math.Cos(angle * Math.PI / 180);

                                for (int j = (int)(adjoinLine.alarm.Perpendicular.X - interval); j >= (adjoinLine.alarm.FootDropLine.X1 < adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j -= (int)interval)
                                {
                                    double pointy = (j - adjoinLine.alarm.FootDropLine.X1) * ((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) + adjoinLine.alarm.FootDropLine.Y1;
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, pointy, -180 + angle);
                                }

                            }

                        }
                        else
                        {
                            if (adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2)
                            {
                                for (int j = (int)adjoinLine.alarm.Perpendicular.X + Pointdistance; j <= (adjoinLine.alarm.FootDropLine.X1 > adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j += Pointdistance)
                                {
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, adjoinLine.alarm.FootDropLine.Y1, 0);
                                }
                            }
                            if (!(adjoinLine.alarm.FootDropLine.X1 == adjoinLine.alarm.FootDropLine.X2) && !(adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2))
                            {
                                double angle = Math.Atan((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) * 180 / Math.PI;
                                double interval = Pointdistance * Math.Cos(angle * Math.PI / 180);

                                for (int j = (int)(adjoinLine.alarm.Perpendicular.X + interval); j <= (adjoinLine.alarm.FootDropLine.X1 > adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j += (int)interval)
                                {
                                    double pointy = (j - adjoinLine.alarm.FootDropLine.X1) * ((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) + adjoinLine.alarm.FootDropLine.Y1;
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, pointy, angle);
                                }

                            }
                        }
                    }
                }
                else
                {
                    if (adjoinLine.alarm.FootDropLine.X1 == adjoinLine.alarm.FootDropLine.X2)
                    {
                        if (point.Y <= adjoinLine.alarm.Perpendicular.Y)
                        {
                            for (int j = (int)adjoinLine.alarm.Perpendicular.Y + Pointdistance; j <= (adjoinLine.alarm.FootDropLine.Y1 > adjoinLine.alarm.FootDropLine.Y2 ? adjoinLine.alarm.FootDropLine.Y1 : adjoinLine.alarm.FootDropLine.Y2); j += Pointdistance)
                            {
                                _context.PrintArrow(adjoinLine.alarm.FootDropLine, adjoinLine.alarm.FootDropLine.X1, j, 90);
                            }
                        }
                        else
                        {
                            for (int j = (int)adjoinLine.alarm.Perpendicular.Y - Pointdistance; j >= (adjoinLine.alarm.FootDropLine.Y1 < adjoinLine.alarm.FootDropLine.Y2 ? adjoinLine.alarm.FootDropLine.Y1 : adjoinLine.alarm.FootDropLine.Y2); j -= Pointdistance)
                            {
                                _context.PrintArrow(adjoinLine.alarm.FootDropLine, adjoinLine.alarm.FootDropLine.X1, j, -90);
                            }
                        }
                    }
                    else
                    {
                        if (point.X <= adjoinLine.alarm.Perpendicular.X)
                        {
                            if (adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2)
                            {
                                for (int j = (int)adjoinLine.alarm.Perpendicular.X + Pointdistance; j <= (adjoinLine.alarm.FootDropLine.X1 > adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j += Pointdistance)
                                {
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, adjoinLine.alarm.FootDropLine.Y1, 0);
                                }
                            }
                            if (!(adjoinLine.alarm.FootDropLine.X1 == adjoinLine.alarm.FootDropLine.X2) && !(adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2))
                            {
                                double angle = Math.Atan((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) * 180 / Math.PI;
                                double interval = Pointdistance * Math.Cos(angle * Math.PI / 180);

                                for (int j = (int)(adjoinLine.alarm.Perpendicular.X + interval); j <= (adjoinLine.alarm.FootDropLine.X1 > adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j += (int)interval)
                                {
                                    double pointy = (j - adjoinLine.alarm.FootDropLine.X1) * ((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) + adjoinLine.alarm.FootDropLine.Y1;
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, pointy, angle);
                                }

                            }

                        }
                        else
                        {
                            if (adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2)
                            {
                                for (int j = (int)adjoinLine.alarm.Perpendicular.X - Pointdistance; j >= (adjoinLine.alarm.FootDropLine.X1 < adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j -= Pointdistance)
                                {
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, adjoinLine.alarm.FootDropLine.Y1, -180);
                                }
                            }
                            if (!(adjoinLine.alarm.FootDropLine.X1 == adjoinLine.alarm.FootDropLine.X2) && !(adjoinLine.alarm.FootDropLine.Y1 == adjoinLine.alarm.FootDropLine.Y2))
                            {
                                double angle = Math.Atan((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) * 180 / Math.PI;
                                double interval = Pointdistance * Math.Cos(angle * Math.PI / 180);

                                for (int j = (int)(adjoinLine.alarm.Perpendicular.X - interval); j >= (adjoinLine.alarm.FootDropLine.X1 < adjoinLine.alarm.FootDropLine.X2 ? adjoinLine.alarm.FootDropLine.X1 : adjoinLine.alarm.FootDropLine.X2); j -= (int)interval)
                                {
                                    double pointy = (j - adjoinLine.alarm.FootDropLine.X1) * ((adjoinLine.alarm.FootDropLine.Y2 - adjoinLine.alarm.FootDropLine.Y1) / (adjoinLine.alarm.FootDropLine.X2 - adjoinLine.alarm.FootDropLine.X1)) + adjoinLine.alarm.FootDropLine.Y1;
                                    _context.PrintArrow(adjoinLine.alarm.FootDropLine, j, pointy, -180 + angle);
                                }

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 路线冲突时更改上一条路线的方向
        /// </summary>
        /// <param name="line">当前线段</param>
        /// <param name="FinPoint">上一条线段的起点</param>
        private void RouteAgain(Line line, Point FinPoint)
        {
            if (!Ends.Contains(FinPoint))
            {
                AlarmPoint alarmpoint = new AlarmPoint();
                List<Line> FrontLines = new List<Line>();
                List<Image> FrontImages = new List<Image>();
                //找出Line的前面节点的路线
                for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                {
                    if (LstEscapeLinesCurrentFloor[i] == null)
                    {
                        continue;
                    }
                    else
                    {
                        Line FrontLine = new Line();
                        FrontLine.Name = LstEscapeLinesCurrentFloor[i].Name;
                        FrontLine.X1 = LstEscapeLinesCurrentFloor[i].TransformX1;
                        FrontLine.X2 = LstEscapeLinesCurrentFloor[i].TransformX2;
                        FrontLine.Y1 = LstEscapeLinesCurrentFloor[i].TransformY1;
                        FrontLine.Y2 = LstEscapeLinesCurrentFloor[i].TransformY2;
                        if (((FrontLine.X1 == FinPoint.X && FrontLine.Y1 == FinPoint.Y) || (FrontLine.X2 == FinPoint.X && FrontLine.Y2 == FinPoint.Y)) && (FrontLine.X1 != line.X1 || FrontLine.X2 != line.X2 || FrontLine.Y1 != line.Y1 || FrontLine.Y2 != line.Y2))
                        {
                            bool IsFootDropLines = false;
                            for (int j = 0; j < FootDropLines.Count; j++)
                            {
                                if (FrontLine.X1 == FootDropLines[j].FootDropLine.X1 && FrontLine.X2 == FootDropLines[j].FootDropLine.X2 && FrontLine.Y1 == FootDropLines[j].FootDropLine.Y1 && FrontLine.Y2 == FootDropLines[j].FootDropLine.Y2)
                                {
                                    IsFootDropLines = true;
                                    break;
                                }
                            }
                            if (!IsFootDropLines)
                            {
                                if (PrintedLines.Find(x => x.Name == FrontLine.Name) != null)
                                {
                                    FrontLine.Tag = PrintedLines.Find(x => x.Name == FrontLine.Name).Tag;
                                }
                                FrontLines.Add(FrontLine);
                            }
                        }
                    }
                }
                int count = FrontLines.Count;

                if (FrontLines.Count != 0)
                {

                    if (FrontLines.Count == 1)
                    {
                        if (FrontLines[0].Tag == null)
                        {
                            Route(FrontLines[0], FinPoint);
                            FinPoint = GetOtherPoint(FrontLines[0], FinPoint);

                            RouteAgain(FrontLines[0], FinPoint);
                        }
                        else
                        {
                            if ((Point)FrontLines[0].Tag != FinPoint)
                            {
                                alarmpoint.FootDropLine = FrontLines[0];
                                _context.DeleteOldImage(alarmpoint);

                                Route(FrontLines[0], FinPoint);
                                FinPoint = GetOtherPoint(FrontLines[0], FinPoint);

                                RouteAgain(FrontLines[0], FinPoint);
                                //如果此路不通呢？？
                            }
                        }

                    }
                    else
                    {
                        if (FrontLines.FindAll(x => x.Tag == null).Count == FrontLines.Count)
                        {
                            JudgeEnd(line, GetOtherPoint(line, FinPoint));
                        }
                        else
                        {
                            FrontLines.RemoveAll(x => x.Tag == null);//部分线路的没有标志，则该线路为不可行，删除该线段
                            if (FrontLines.FindAll(x => (Point)x.Tag == FinPoint).Count == 0)
                            {
                                bool IsExist = false;
                                int index = FrontLines.Count;
                                for (int i = 0; i < FrontLines.Count; i++)
                                {
                                    Point finPoint = (Point)FrontLines[i].Tag;
                                    ChaRouteDirection.Clear();
                                    IsExist = JudgeDirection(FrontLines[i], finPoint);
                                    if (IsExist)
                                    {
                                        index = i;
                                        break;
                                    }
                                }
                                if (index != FrontLines.Count)
                                {
                                    alarmpoint.FootDropLine = FrontLines[index];
                                    _context.DeleteOldImage(alarmpoint);
                                    Route(FrontLines[index], FinPoint);

                                    FinPoint = GetOtherPoint(FrontLines[index], FinPoint);

                                    RouteAgain(FrontLines[index], FinPoint);
                                }
                                else
                                {
                                    //此路不通的情况！！！
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 寻找下一条线段标志箭头
        /// </summary>
        /// <param name="line">目前线段</param>
        /// <param name="CurValue">在目前线段上下一个箭头的坐标数值</param>
        /// <param name="ComValue">与下一个箭头坐标数值比较的数值</param>
        /// <param name="ChaValue">箭头之间坐标变化的数值</param>
        private void NextPrint(Line line, double CurValue, double ComValue, double ChaValue)
        {
            Point nextPoint = new Point();
            if (line.X1 == line.X2)
            {
                if (line.Y1 == ComValue)
                {
                    nextPoint.X = line.X2;
                    nextPoint.Y = line.Y2;
                }
                else
                {
                    nextPoint.X = line.X1;
                    nextPoint.Y = line.Y1;
                }
            }
            else
            {
                if (line.X1 == ComValue)
                {
                    nextPoint.X = line.X2;
                    nextPoint.Y = line.Y2;
                }
                else
                {
                    nextPoint.X = line.X1;
                    nextPoint.Y = line.Y1;
                }
            }

            if (ChaValue > 0)
            {
                if ((CurValue + ChaValue) >= ComValue)
                {
                    if (LstPlanPartitionPointCurrent.Count > 1)
                    {
                        JudgeEndAgain(line, nextPoint);
                    }
                    else
                    {
                        JudgeEnd(line, nextPoint);
                    }
                }
            }
            else
            {
                if ((CurValue + ChaValue) <= ComValue)
                {
                    if (LstPlanPartitionPointCurrent.Count > 1)
                    {
                        JudgeEndAgain(line, nextPoint);
                    }
                    else
                    {
                        JudgeEnd(line, nextPoint);
                    }
                }

            }
        }

        /// <summary>
        /// 重新绘画线段
        /// </summary>
        /// <param name="line">需要重新设置方向的线段</param>
        /// <param name="StartPoint">该线段的新起点</param>
        private void Route(Line line, Point StartPoint)
        {
            if (FootDropLines.FindAll(x => x.FootDropLine == line).Count != 0)
            {
                line.Tag = null;
            }
            else
            {
                line.Tag = StartPoint;//标记线段的方向
            }
            if (PrintedLines.Find(x => x.Name == line.Name) != null)
            {
                PrintedLines.Find(x => x.Name == line.Name).Tag = StartPoint;
            }
            Point EndPoint = GetOtherPoint(line, StartPoint);

            if (StartPoint.X == EndPoint.X)
            {
                if (StartPoint.Y > EndPoint.Y)
                {
                    for (int i = (int)StartPoint.Y - Pointdistance; i >= EndPoint.Y; i -= Pointdistance)
                    {
                        _context.PrintArrow(line, StartPoint.X, i, -90);
                    }
                }
                else
                {
                    for (int i = (int)StartPoint.Y + Pointdistance; i <= EndPoint.Y; i += Pointdistance)
                    {
                        _context.PrintArrow(line, StartPoint.X, i, 90);
                    }
                }
            }

            if (StartPoint.Y == EndPoint.Y)
            {
                if (StartPoint.X > EndPoint.X)
                {
                    for (int i = (int)StartPoint.X - Pointdistance; i >= EndPoint.X; i -= Pointdistance)
                    {
                        _context.PrintArrow(line, i, StartPoint.Y, 180);
                    }
                }
                else
                {
                    for (int i = (int)StartPoint.X + Pointdistance; i <= EndPoint.X; i += Pointdistance)
                    {
                        _context.PrintArrow(line, i, StartPoint.Y, 0);
                    }
                }
            }

            if (StartPoint.X != EndPoint.X && StartPoint.Y != EndPoint.Y)
            {
                double k = (StartPoint.Y - EndPoint.Y) / (StartPoint.X - EndPoint.X);
                double angle = Math.Atan((StartPoint.Y - EndPoint.Y) / (StartPoint.X - EndPoint.X)) * 180 / Math.PI;
                double interval = Pointdistance * Math.Cos(angle * Math.PI / 180);
                if (k < 0)
                {
                    if (StartPoint.X < EndPoint.X)
                    {
                        for (int i = (int)(StartPoint.X + interval); i <= EndPoint.X; i += (int)interval)
                        {
                            _context.PrintArrow(line, i, (i - StartPoint.X) * k + StartPoint.Y, angle);
                        }
                    }
                    else
                    {
                        for (int i = (int)(StartPoint.X - interval); i >= EndPoint.X; i -= (int)interval)
                        {
                            _context.PrintArrow(line, i, (i - StartPoint.X) * k + StartPoint.Y, angle - 180);
                        }
                    }
                }
                if (k > 0)
                {
                    if (StartPoint.X < EndPoint.X)
                    {
                        for (int i = (int)(StartPoint.X + interval); i <= EndPoint.X; i += (int)interval)
                        {
                            _context.PrintArrow(line, i, (i - StartPoint.X) * k + StartPoint.Y, angle);
                        }
                    }
                    else
                    {
                        for (int i = (int)(StartPoint.X - interval); i >= EndPoint.X; i -= (int)interval)
                        {
                            _context.PrintArrow(line, i, (i - StartPoint.X) * k + StartPoint.Y, angle - 180);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 重新规划指定线段，掉反线段方向
        /// </summary>
        /// <param name="line">指定线段</param>
        /// <param name="point">改变后下一条线路的起点</param>
        /// <returns></returns>
        private bool JudgeDirection(Line line, Point point)
        {
            if (Ends.Contains(point))
            {
                if (FootDropLines.FindAll(x => x.Perpendicular == point).Count == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                List<Line> FrontLines = new List<Line>();
                for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                {
                    if (LstEscapeLinesCurrentFloor[i] == null)
                    {
                        continue;
                    }
                    else
                    {
                        Line FrontLine = new Line();
                        FrontLine.Name = LstEscapeLinesCurrentFloor[i].Name;
                        FrontLine.X1 = LstEscapeLinesCurrentFloor[i].TransformX1;
                        FrontLine.X2 = LstEscapeLinesCurrentFloor[i].TransformX2;
                        FrontLine.Y1 = LstEscapeLinesCurrentFloor[i].TransformY1;
                        FrontLine.Y2 = LstEscapeLinesCurrentFloor[i].TransformY2;
                        if (((FrontLine.X1 == point.X && FrontLine.Y1 == point.Y) || (FrontLine.X2 == point.X && FrontLine.Y2 == point.Y)) && (FrontLine.X1 != line.X1 || FrontLine.X2 != line.X2 || FrontLine.Y1 != line.Y1 || FrontLine.Y2 != line.Y2) && ChaRouteDirection.FindAll(x => x.X1 == FrontLine.X1 && x.X2 == FrontLine.X2 && x.Y1 == FrontLine.Y1 && x.Y2 == FrontLine.Y2).Count == 0)
                        {
                            bool IsFootDropLines = false;
                            for (int j = 0; j < FootDropLines.Count; j++)
                            {
                                if (FrontLine.X1 == FootDropLines[j].FootDropLine.X1 && FrontLine.X2 == FootDropLines[j].FootDropLine.X2 && FrontLine.Y1 == FootDropLines[j].FootDropLine.Y1 && FrontLine.Y2 == FootDropLines[j].FootDropLine.Y2)
                                {
                                    IsFootDropLines = true;
                                    break;
                                }
                            }
                            if (!IsFootDropLines)
                            {
                                FrontLines.Add(FrontLine);
                            }
                        }
                    }
                }

                for (int i = 0; i < FrontLines.Count; i++)
                {
                    Line lined = PrintedLines.Find(x => x.X1 == FrontLines[i].X1 && x.X2 == FrontLines[i].X2 && x.Y1 == FrontLines[i].Y1 && x.Y2 == FrontLines[i].Y2);
                    if (lined != null)
                    {
                        FrontLines[i].Tag = lined.Tag;
                    }
                }

                //判断附近线段上存不存在箭头，不存在则此路不通，删除该线段
                if (FrontLines.FindAll(x => x.Tag == null).Count != FrontLines.Count)
                {
                    for (int i = 0; i < FrontLines.Count; i++)
                    {
                        if (!IsExistArrowImage(FrontLines[i]))
                        {
                            FrontLines.RemoveAt(i);
                            i = -1;
                        }
                    }
                }

                for (int i = 0; i < FrontLines.Count; i++)
                {
                    Point NextPoint = new Point();

                    NextPoint = GetOtherPoint(FrontLines[i], point);

                    if (Ends.Contains(NextPoint))
                    {
                        if (FootDropLines.FindAll(x => x.Perpendicular == NextPoint).Count == 0)
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (FrontLines[i].Tag == null)
                        {
                            if (JudgeIsEnd(FrontLines[i], NextPoint))
                            {
                                return true;
                            }
                            else
                            {
                                ChaRouteDirection.Add(FrontLines[i]);
                                continue;
                            }
                        }
                        else
                        {
                            if ((Point)FrontLines[i].Tag == point)
                            {
                                ChaRouteDirection.Add(FrontLines[i]);
                                if (JudgeDirection(FrontLines[i], NextPoint))
                                {
                                    return true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                ChaRouteDirection.Add(FrontLines[i]);
                                Point FinPoint = (Point)FrontLines[i].Tag;
                                if (Ends.Contains(FinPoint) && FootDropLines.FindAll(x => x.Perpendicular == FinPoint).Count != 0)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (JudgeDirection(FrontLines[i], FinPoint))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                #region 旧方法
                if (FrontLines.Count != 0)
                {
                    if (FrontLines.Count == 1 && ChaRouteDirection.FindAll(x => x.X1 == FrontLines[0].X1 && x.X2 == FrontLines[0].X2 && x.Y1 == FrontLines[0].Y1 && x.Y2 == FrontLines[0].Y2).Count == 0)
                    {
                        if ((Point)FrontLines[0].Tag == point)
                        {
                            Point FinPoint = GetOtherPoint(FrontLines[0], (Point)FrontLines[0].Tag);

                            if (Ends.Contains(FinPoint))
                            {
                                if (FootDropLines.FindAll(x => x.Perpendicular == FinPoint).Count == 0)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                ChaRouteDirection.Add(FrontLines[0]);
                                return JudgeDirection(FrontLines[0], FinPoint);
                            }
                        }
                        else
                        {
                            ChaRouteDirection.Add(FrontLines[0]);
                            return JudgeDirection(FrontLines[0], (Point)FrontLines[0].Tag);
                        }
                    }
                    if (FrontLines.Count > 1)
                    {
                        for (int i = 0; i < FrontLines.Count; i++)
                        {
                            if (ChaRouteDirection.FindAll(x => x.X1 == FrontLines[i].X1 && x.X2 == FrontLines[i].X2 && x.Y1 == FrontLines[i].Y1 && x.Y2 == FrontLines[i].Y2).Count > 0)
                            {
                                continue;
                            }
                            if ((Point)FrontLines[i].Tag == point)
                            {
                                Point FinPoint = GetOtherPoint(FrontLines[i], (Point)FrontLines[i].Tag);

                                if (Ends.Contains(FinPoint))
                                {
                                    if (FootDropLines.FindAll(x => x.Perpendicular == FinPoint).Count == 0)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    ChaRouteDirection.Add(FrontLines[i]);
                                    if (JudgeDirection(FrontLines[i], FinPoint))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                ChaRouteDirection.Add(FrontLines[i]);
                                Point FinPoint = (Point)FrontLines[i].Tag;
                                if (Ends.Contains(FinPoint) && FootDropLines.FindAll(x => x.Perpendicular == FinPoint).Count != 0)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (JudgeDirection(FrontLines[i], FinPoint))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
                #endregion
                return false;
            }
            //return true;
        }

        /// <summary>
        /// 求出线段的另一端坐标
        /// </summary>
        /// <param name="line">所求的线段</param>
        /// <param name="point">已知端点</param>
        /// <returns></returns>
        private Point GetOtherPoint(Line line, Point point)
        {
            Point result = new Point();
            if (line.X1 == point.X && line.Y1 == point.Y)
            {
                result.X = line.X2;
                result.Y = line.Y2;
            }
            else
            {
                result.X = line.X1;
                result.Y = line.Y1;
            }

            return result;
        }

        /// <summary>
        /// 绘画起点线段以后的线段箭头
        /// </summary>
        /// <param name="line">新增报警点对应的线段</param>
        /// <param name="b">其他相邻线段的起点标记</param>
        private void JudgeEnd(Line line, Point point)
        {
            if (OldNextLines.Count != 0 && NewNextLines.Count > 1)
            {
                if (OldNextLines[OldNextLines.Count - 1].nextline.TransformX1 == line.X1 && OldNextLines[OldNextLines.Count - 1].nextline.TransformX2 == line.X2 && OldNextLines[OldNextLines.Count - 1].nextline.TransformY1 == line.Y1 && OldNextLines[OldNextLines.Count - 1].nextline.TransformY2 == line.Y2)
                {
                    Point startpoint = OldNextLines[OldNextLines.Count - 1].StartingPoint;
                    OldNextLines.RemoveAt(OldNextLines.Count - 1);
                    if (OldNextLines.Count > 0 && Corners[Corners.Count - 1] == startpoint && OldNextLines[OldNextLines.Count - 1].StartingPoint != startpoint)
                    {
                        Corners.RemoveAt(Corners.Count - 1);
                    }
                }
                else
                {
                    OldNextLines.Remove(OldNextLines.Find(x => x.nextline.TransformX1 == line.X1 && x.nextline.TransformX2 == line.X2 && x.nextline.TransformY1 == line.Y1 && x.nextline.TransformY2 == line.Y2));
                }
            }
            nextLines nextLine;
            //求出线段另一端点的坐标
            Point corner = new Point();
            EscapeLinesInfo infoEscapeLine = LstEscapeLinesCurrentFloor.Find(x => x.TransformX1 == line.X1 && x.TransformX2 == line.X2 && x.TransformY1 == line.Y1 && x.TransformY2 == line.Y2);
            if ((point.X == infoEscapeLine.LineX1 && point.Y == infoEscapeLine.LineY1) || (point.X == infoEscapeLine.LineX2 && point.Y == infoEscapeLine.LineY2))
            {
                if (point.X == infoEscapeLine.LineX1 && point.Y == infoEscapeLine.LineY1)
                {
                    corner = GetOtherPoint(line, new Point(infoEscapeLine.TransformX1, infoEscapeLine.TransformY1));
                }
                else
                {
                    corner = GetOtherPoint(line, new Point(infoEscapeLine.TransformX2, infoEscapeLine.TransformY2));
                }
            }
            else
            {
                corner = GetOtherPoint(line, point);//放大缩小坐标
            }

            if (!Ends.Contains(corner))
            {
                Point Ocorner = new Point();//线段端点原始坐标
                if (corner.X == infoEscapeLine.TransformX1 && corner.Y == infoEscapeLine.TransformY1)
                {
                    Ocorner.X = infoEscapeLine.LineX1;
                    Ocorner.Y = infoEscapeLine.LineY1;
                }
                else
                {
                    Ocorner.X = infoEscapeLine.LineX2;
                    Ocorner.Y = infoEscapeLine.LineY2;
                }
                NewNextLines.Clear();
                for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                {
                    Line NextLine = new Line();
                    NextLine.Name = LstEscapeLinesCurrentFloor[i].Name;
                    NextLine.X1 = LstEscapeLinesCurrentFloor[i].LineX1;
                    NextLine.X2 = LstEscapeLinesCurrentFloor[i].LineX2;
                    NextLine.Y1 = LstEscapeLinesCurrentFloor[i].LineY1;
                    NextLine.Y2 = LstEscapeLinesCurrentFloor[i].LineY2;
                    if (NextLine == null || PrintedLines.FindAll(x => x.X1 == LstEscapeLinesCurrentFloor[i].TransformX1 && x.X2 == LstEscapeLinesCurrentFloor[i].TransformX2 && x.Y1 == LstEscapeLinesCurrentFloor[i].TransformY1 && x.Y2 == LstEscapeLinesCurrentFloor[i].TransformY2).Count != 0)
                    {
                        continue;
                    }
                    else
                    {
                        if ((NextLine.X1 == Ocorner.X && NextLine.Y1 == Ocorner.Y) || (NextLine.X2 == Ocorner.X && NextLine.Y2 == Ocorner.Y))
                        {
                            nextLine.nextline = LstEscapeLinesCurrentFloor[i];
                            nextLine.StartingPoint = corner;
                            NewNextLines.Add(nextLine);
                        }
                    }
                }
                if (NewNextLines.Count > 1)
                {
                    OldNextLines.AddRange(NewNextLines);
                    Corners.Add(corner);

                    for (int i = OldNextLines.Count - 1; i >= 0; i--)
                    {
                        if (Corners.Count == 0)
                        {
                            OldNextLines.Remove(OldNextLines[0]);
                            i = OldNextLines.Count;
                        }
                        else
                        {
                            if (PrintedLines.FindAll(x => x.X1 == OldNextLines[i].nextline.TransformX1 && x.X2 == OldNextLines[i].nextline.TransformX2 && x.Y1 == OldNextLines[i].nextline.TransformY1 && x.Y2 == OldNextLines[i].nextline.TransformY2).Count > 0)
                            {
                                Point startpoint = OldNextLines[i].StartingPoint;
                                OldNextLines.RemoveAt(i);
                                if (i > 0 && Corners[Corners.Count - 1] == startpoint && OldNextLines[i - 1].StartingPoint != startpoint)
                                {
                                    Point LastCorner = Corners[Corners.Count - 1];
                                    if (OldNextLines.FindAll(x => x.StartingPoint == startpoint).Count == 0)
                                    {
                                        Corners.RemoveAll(x => x == LastCorner);
                                    }
                                    else
                                    {
                                        Corners.RemoveAt(Corners.Count - 1);
                                        while (Corners.Count > 0 && Corners[Corners.Count - 1] == LastCorner)
                                        {
                                            Corners.RemoveAt(Corners.Count - 1);
                                        }
                                    }
                                }
                                i = OldNextLines.Count;
                            }
                            else
                            {
                                if (OldNextLines[i].StartingPoint == Corners[Corners.Count - 1])
                                {
                                    Line newline = new Line();
                                    Line oldline = new Line();
                                    newline.Name = OldNextLines[i].nextline.Name;
                                    newline.X1 = OldNextLines[i].nextline.TransformX1;
                                    newline.X2 = OldNextLines[i].nextline.TransformX2;
                                    newline.Y1 = OldNextLines[i].nextline.TransformY1;
                                    newline.Y2 = OldNextLines[i].nextline.TransformY2;
                                    oldline.Name = OldNextLines[i].nextline.Name;
                                    oldline.X1 = OldNextLines[i].nextline.LineX1;
                                    oldline.X2 = OldNextLines[i].nextline.LineX2;
                                    oldline.Y1 = OldNextLines[i].nextline.LineY1;
                                    oldline.Y2 = OldNextLines[i].nextline.LineY2;
                                    Point endPoint = GetOtherPoint(newline, OldNextLines[i].StartingPoint);

                                    //删除已经处理的转折点
                                    if (i == 0)
                                    {
                                        Point LastCorner = Corners[Corners.Count - 1];
                                        Corners.RemoveAt(Corners.Count - 1);
                                        if (Corners.Count > 0 && Corners[Corners.Count - 1] == LastCorner)
                                        {
                                            Corners.RemoveAt(Corners.Count - 1);
                                        }
                                    }
                                    if (OldNextLines.FindAll(x => x.StartingPoint == endPoint).Count == 0)
                                    {
                                        Point NextEnd = GetOtherPoint(newline, OldNextLines[i].StartingPoint);
                                        Point OldNextEnd = GetOtherPoint(oldline, OldNextLines[i].StartingPoint);

                                        if (Ends.Contains(NextEnd))
                                        {
                                            if (NextEnd == FootDropLines[0].Perpendicular && Ends.Count > 1)
                                            {
                                                PrintNext(newline, NextEnd);
                                            }
                                            else
                                            {
                                                PrintNext(newline, OldNextLines[i].StartingPoint);
                                            }
                                        }
                                        else
                                        {
                                            bool IsExistLine = false;
                                            for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                                            {
                                                if (LstEscapeLinesCurrentFloor[j] == null)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    Line aline = new Line();
                                                    aline.Name = LstEscapeLinesCurrentFloor[j].Name;
                                                    aline.X1 = LstEscapeLinesCurrentFloor[j].LineX1;
                                                    aline.X2 = LstEscapeLinesCurrentFloor[j].LineX2;
                                                    aline.Y1 = LstEscapeLinesCurrentFloor[j].LineY1;
                                                    aline.Y2 = LstEscapeLinesCurrentFloor[j].LineY2;
                                                    if ((aline.X1 != OldNextLines[i].nextline.LineX1 || aline.X2 != OldNextLines[i].nextline.LineX2 || aline.Y1 != OldNextLines[i].nextline.LineY1 || aline.Y2 != OldNextLines[i].nextline.LineY2) && ((aline.X1 == OldNextEnd.X && aline.Y1 == OldNextEnd.Y) || (aline.X2 == OldNextEnd.X && aline.Y2 == OldNextEnd.Y)))
                                                    {
                                                        IsExistLine = true;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (IsExistLine)
                                            {
                                                for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                                                {
                                                    if (OldNextLines.Count != 0)
                                                    {
                                                        if (LstEscapeLinesCurrentFloor[j] == null)
                                                        {
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            if ((LstEscapeLinesCurrentFloor[j].TransformX1 == NextEnd.X && LstEscapeLinesCurrentFloor[j].TransformY1 == NextEnd.Y) || (LstEscapeLinesCurrentFloor[j].TransformX2 == NextEnd.X && LstEscapeLinesCurrentFloor[j].TransformY2 == NextEnd.Y))
                                                            {
                                                                if ((LstEscapeLinesCurrentFloor[j].TransformX1 == PrintedLines[0].X1 && LstEscapeLinesCurrentFloor[j].TransformX2 == PrintedLines[0].X2 && LstEscapeLinesCurrentFloor[j].TransformY1 == PrintedLines[0].Y1 && LstEscapeLinesCurrentFloor[j].TransformY2 == PrintedLines[0].Y2) && !Ends.Contains(NextEnd))
                                                                {
                                                                    PrintNext(newline, NextEnd);
                                                                }

                                                            }
                                                        }

                                                        if (i < OldNextLines.Count && j + 1 == LstEscapeLinesCurrentFloor.Count && PrintedLines.FindAll(x => x.X1 == OldNextLines[i].nextline.TransformX1 && x.X2 == OldNextLines[i].nextline.TransformX2 && x.Y1 == OldNextLines[i].nextline.TransformY1 && x.Y2 == OldNextLines[i].nextline.TransformY2).Count == 0)
                                                        {
                                                            PrintNext(newline, OldNextLines[i].StartingPoint);
                                                        }
                                                    }

                                                }
                                                i = OldNextLines.Count;
                                            }
                                            else
                                            {
                                                PrintNext(newline, NextEnd);
                                            }
                                        }
                                        i = OldNextLines.Count;
                                    }
                                    else
                                    {
                                        Point startpoint = OldNextLines[i].StartingPoint;
                                        OldNextLines.RemoveAt(i);
                                        if (i > 0 && Corners[Corners.Count - 1] == startpoint && OldNextLines[i - 1].StartingPoint != startpoint)
                                        {
                                            Corners.RemoveAt(Corners.Count - 1);
                                        }
                                        i = OldNextLines.Count;
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (NewNextLines.Count != 0)
                    {
                        Line newline = new Line();
                        Point Opoint = new Point();
                        newline.Name = NewNextLines[0].nextline.Name;
                        newline.X1 = NewNextLines[0].nextline.TransformX1;
                        newline.X2 = NewNextLines[0].nextline.TransformX2;
                        newline.Y1 = NewNextLines[0].nextline.TransformY1;
                        newline.Y2 = NewNextLines[0].nextline.TransformY2;
                        if (Ocorner.X == NewNextLines[0].nextline.LineX1 && Ocorner.Y == NewNextLines[0].nextline.LineY1)
                        {
                            Opoint.X = NewNextLines[0].nextline.TransformX1;
                            Opoint.Y = NewNextLines[0].nextline.TransformY1;
                        }
                        else
                        {
                            Opoint.X = NewNextLines[0].nextline.TransformX2;
                            Opoint.Y = NewNextLines[0].nextline.TransformY2;
                        }

                        if (Opoint.X < 210 || Opoint.X > 950 || Opoint.Y < 132 || Opoint.Y > 582)//判断下一条线段是否在显示范围内
                        {
                            Line oldline = new Line();
                            oldline.X1 = NewNextLines[0].nextline.LineX1;
                            oldline.X2 = NewNextLines[0].nextline.LineX2;
                            oldline.Y1 = NewNextLines[0].nextline.LineY1;
                            oldline.Y2 = NewNextLines[0].nextline.LineY2;
                            if (PrintedLines.FindAll(x => x.X1 == newline.X1 && x.X2 == newline.X2 && x.Y1 == newline.Y1 && x.Y2 == newline.Y2).Count == 0)
                            {
                                PrintedLines.Add(newline);
                            }
                            JudgeEnd(newline, Ocorner);
                        }
                        else
                        {
                            Point NextEnd = GetOtherPoint(newline, NewNextLines[0].StartingPoint);

                            for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                            {
                                if (LstEscapeLinesCurrentFloor[j] == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    if ((LstEscapeLinesCurrentFloor[j].TransformX1 == NextEnd.X && LstEscapeLinesCurrentFloor[j].TransformY1 == NextEnd.Y) || (LstEscapeLinesCurrentFloor[j].TransformX2 == NextEnd.X && LstEscapeLinesCurrentFloor[j].TransformY2 == NextEnd.Y))
                                    {
                                        if ((LstEscapeLinesCurrentFloor[j].TransformX1 == PrintedLines[0].X1 && LstEscapeLinesCurrentFloor[j].TransformX2 == PrintedLines[0].X2 && LstEscapeLinesCurrentFloor[j].TransformY1 == PrintedLines[0].Y1 && LstEscapeLinesCurrentFloor[j].TransformY2 == PrintedLines[0].Y2) && !Ends.Contains(NextEnd))
                                        {
                                            PrintNext(newline, NextEnd);
                                        }
                                    }
                                }

                                if (j + 1 == LstEscapeLinesCurrentFloor.Count && PrintedLines.FindAll(x => x.X1 == NewNextLines[0].nextline.TransformX1 && x.X2 == NewNextLines[0].nextline.TransformX2 && x.Y1 == NewNextLines[0].nextline.TransformY1 && x.Y2 == NewNextLines[0].nextline.TransformY2).Count == 0)
                                {
                                    Point nextpoint = GetHidPoint(NewNextLines[0].StartingPoint);
                                    PrintNext(newline, nextpoint);
                                }
                            }
                        }
                    }
                    else//没有出口
                    {
                        bool IsExitLine = false;
                        for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                        {
                            if (LstEscapeLinesCurrentFloor[i] == null)
                            {
                                continue;
                            }
                            else
                            {
                                if ((LstEscapeLinesCurrentFloor[i].TransformX1 != line.X1 || LstEscapeLinesCurrentFloor[i].TransformX2 != line.X2 || LstEscapeLinesCurrentFloor[i].TransformY1 != line.Y1 || LstEscapeLinesCurrentFloor[i].TransformY2 != line.Y2) && (Ocorner.X == LstEscapeLinesCurrentFloor[i].LineX1 && (Ocorner.Y == LstEscapeLinesCurrentFloor[i].LineY1) || (Ocorner.X == LstEscapeLinesCurrentFloor[i].LineX2 && Ocorner.Y == LstEscapeLinesCurrentFloor[i].LineY2)))
                                {
                                    IsExitLine = true;
                                    break;
                                }
                            }
                        }

                        if (!IsExitLine)
                        {
                            AlarmPoint alarm = new AlarmPoint();
                            alarm.FootDropLine = line;
                            //删除当前线段的方向标志
                            _context.DeleteOldImage(alarm);
                            Route(line, corner);
                            RouteAgain(line, point);
                        }

                    }
                    #region
                    //else
                    //{
                    //    Point oldpoint = new Point();
                    //    Line oldline = new Line();
                    //    EscapeLinesInfo infoEscapeLine = LstEscapeLinesCurrentFloor.Find(x => x.TransformX1 == line.X1 && x.TransformX2 == line.X2 && x.TransformY1 == line.Y1 && x.TransformY2 == line.Y2);
                    //    oldline.X1 = infoEscapeLine.LineX1;
                    //    oldline.X2 = infoEscapeLine.LineX2;
                    //    oldline.Y1 = infoEscapeLine.LineY1;
                    //    oldline.Y2 = infoEscapeLine.LineY2;
                    //    if(corner.X == infoEscapeLine.TransformX1 && corner.Y == infoEscapeLine.TransformY1)
                    //    {
                    //        oldpoint.X = infoEscapeLine.LineX1;
                    //        oldpoint.Y = infoEscapeLine.LineY1;
                    //    }
                    //    else
                    //    {
                    //        oldpoint.X = infoEscapeLine.LineX2;
                    //        oldpoint.Y = infoEscapeLine.LineY2;
                    //    }

                    //    for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                    //    {
                    //        Line aline = new Line();
                    //        aline.X1 = LstEscapeLinesCurrentFloor[i].LineX1;
                    //        aline.X2 = LstEscapeLinesCurrentFloor[i].LineX2;
                    //        aline.Y1 = LstEscapeLinesCurrentFloor[i].LineY1;
                    //        aline.Y2 = LstEscapeLinesCurrentFloor[i].LineY2;
                    //        if(aline == null || PrintedLines.FindAll(x => x.X1 == LstEscapeLinesCurrentFloor[i].TransformX1 && x.X2 == LstEscapeLinesCurrentFloor[i].TransformX2 && x.Y1 == LstEscapeLinesCurrentFloor[i].TransformY1 && x.Y2 == LstEscapeLinesCurrentFloor[i].TransformY2).Count != 0)
                    //        {
                    //            continue;
                    //        }
                    //        else
                    //        {
                    //            if ((aline.X1 == oldpoint.X && aline.Y1 == oldpoint.Y) || (aline.X2 == oldpoint.X && aline.Y2 == oldpoint.Y))
                    //            {
                    //                nextLine.nextline = LstEscapeLinesCurrentFloor[i];
                    //                nextLine.StartingPoint = oldpoint;
                    //            }
                    //        }

                    //    }
                    //    if()//有线段不在显示范围内
                    //}
                    #endregion
                }
            }
        }

        /// <summary>
        /// 端点不在显示范围内时改变坐标划线
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Point GetHidPoint(Point point)
        {
            if (point.X < 223)
            {
                point.X = 223;
            }
            if (point.X > 960)
            {
                point.X = 960;
            }
            if (point.Y < 135)
            {
                point.Y = 135;
            }
            if (point.Y > 584)
            {
                point.Y = 584;
            }
            return point;
        }

        /// <summary>
        /// 多个报警点时部分线路重新划分方向
        /// </summary>
        /// <param name="line">报警点附近的线段</param>
        /// <param name="b">线段方向结束的转折点标志</param>
        private void JudgeEndAgain(Line line, Point point)
        {
            //bool IsAdopt = false;//判断此线路是否通过
            List<Line> oldLines = new List<Line>();
            List<Line> OldLines = new List<Line>();
            #region 判断该转折点是1还是2
            Point corner = GetOtherPoint(line, point);

            #endregion
            if (!Ends.Contains(corner))
            {
                for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                {
                    Line oldline = new Line();
                    oldline.Name = LstEscapeLinesCurrentFloor[i].Name;
                    oldline.X1 = LstEscapeLinesCurrentFloor[i].TransformX1;
                    oldline.X2 = LstEscapeLinesCurrentFloor[i].TransformX2;
                    oldline.Y1 = LstEscapeLinesCurrentFloor[i].TransformY1;
                    oldline.Y2 = LstEscapeLinesCurrentFloor[i].TransformY2;
                    if (LstEscapeLinesCurrentFloor[i] == null || FootDropLinesPrinted.FindAll(x => x.FootDropLine.X1 == LstEscapeLinesCurrentFloor[i].TransformX1 && x.FootDropLine.X2 == LstEscapeLinesCurrentFloor[i].TransformX2 && x.FootDropLine.Y1 == LstEscapeLinesCurrentFloor[i].TransformY1 && x.FootDropLine.Y2 == LstEscapeLinesCurrentFloor[i].TransformY2).Count != 0)
                    {
                        continue;
                    }
                    else
                    {
                        if ((oldline.X1 != line.X1 || oldline.X2 != line.X2 || oldline.Y1 != line.Y1 || oldline.Y2 != line.Y2) && ((oldline.X1 == corner.X && oldline.Y1 == corner.Y) || (oldline.X2 == corner.X && oldline.Y2 == corner.Y)))
                        {
                            OldLines.Add(oldline);
                        }
                    }
                }

                if (OldLines.FindAll(x => x.Tag == null).Count == OldLines.Count)
                {
                    JudgeEnd(line, GetOtherPoint(line, corner));
                    //if (JudgeDirection(line,corner))
                    //{
                    //    JudgeEnd(line, corner);
                    //}
                    //else
                    //{
                    //    JudgeEnd(line, GetOtherPoint(line, corner));
                    //}
                }
                else
                {
                    OldLines.RemoveAll(x => x.Tag == null);
                    oldLines = OldLines;

                    List<Image> FrontImages = new List<Image>();
                    if (oldLines.Count == 1)
                    {
                        AlarmPoint alarmpoint = new AlarmPoint();
                        alarmpoint.FootDropLine = oldLines[0];
                        _context.DeleteOldImage(alarmpoint);

                        Route(oldLines[0], corner);

                        corner = GetOtherPoint(oldLines[0], corner);

                        RouteAgain(oldLines[0], corner);
                    }
                    else
                    {
                        if (oldLines.FindAll(x => (Point)x.Tag == corner).Count == 0)
                        {
                            bool IsExist = false;
                            int index = oldLines.Count;
                            for (int i = 0; i < oldLines.Count; i++)
                            {
                                Point finPoint = (Point)oldLines[i].Tag;
                                ChaRouteDirection.Clear();
                                IsExist = JudgeDirection(oldLines[i], finPoint);
                                if (IsExist)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            if (index != oldLines.Count)
                            {
                                RouteAgain(oldLines[index], corner);
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 判断某条线段上是否存在箭头
        /// </summary>
        /// <param name="line">需要判断的线段</param>
        /// <returns></returns>
        private bool IsExistArrowImage(Line line)
        {
            bool IsExist = false;

            for (int i = 0; i < _context.cvsMainWindow.Children.Count; i++)
            {
                Image image = _context.cvsMainWindow.Children[i] as Image;
                if (image == null)
                {
                    continue;
                }
                else
                {
                    if (image.Tag is Line && (image.Tag as Line).X1 == line.X1 && (image.Tag as Line).X2 == line.X2 && (image.Tag as Line).Y1 == line.Y1 && (image.Tag as Line).Y2 == line.Y2)
                    {
                        IsExist = true;
                        break;
                    }
                }
            }

            return IsExist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool JudgeIsEnd(Line line, Point point)
        {
            if(Ends.Contains(point))
            {
                return true;
            }
            else
            {
                List<Line> NextLines = new List<Line>();
                for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                {
                    if (LstEscapeLinesCurrentFloor[i] == null)
                    {
                        continue;
                    }
                    else
                    {
                        Line nextline = new Line();
                        nextline.Name = LstEscapeLinesCurrentFloor[i].Name;
                        nextline.X1 = LstEscapeLinesCurrentFloor[i].TransformX1;
                        nextline.X2 = LstEscapeLinesCurrentFloor[i].TransformX2;
                        nextline.Y1 = LstEscapeLinesCurrentFloor[i].TransformY1;
                        nextline.Y2 = LstEscapeLinesCurrentFloor[i].TransformY2;
                        if ((nextline.X1 != line.X1 || nextline.X2 != line.X2 || nextline.Y1 != line.Y1 || nextline.Y2 != line.Y2) && AlreadyJudgeLine.FindAll(x => x.X1 == nextline.X1 && x.X2 == nextline.X2 && x.Y1 == nextline.Y1 && x.Y2 == nextline.Y2).Count == 0 && FootDropLines.FindAll(x => x.FootDropLine.X1 == nextline.X1 && x.FootDropLine.X2 == nextline.X2 && x.FootDropLine.Y1 == nextline.Y1 && x.FootDropLine.Y2 == nextline.Y2).Count == 0 && ((nextline.X1 == point.X && nextline.Y1 == point.Y) || (nextline.X2 == point.X && nextline.Y2 == point.Y)))
                        {
                            NextLines.Add(nextline);
                        }
                    }
                }

                Point NextPoint = new Point();
                for (int i = 0; i < NextLines.Count; i++)
                {
                    NextPoint = GetOtherPoint(NextLines[i], point);

                    if (Ends.Contains(NextPoint))
                    {
                        return true;
                    }
                    else
                    {
                        AlreadyJudgeLine.Add(NextLines[i]);
                        if (JudgeIsEnd(NextLines[i], NextPoint))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 绘画线段箭头并且查看下一条线段
        /// </summary>
        /// <param name="line">需要绘画的线段</param>
        /// <param name="point">绘画箭头的起点</param>
        private void PrintNext(Line line, Point point)
        {
            if (PrintedLines.FindAll(x => x.X1 == line.X1 && x.X2 == line.X2 && x.Y1 == line.Y1 && x.Y2 == line.Y2).Count == 0)
            {
                PrintedLines.Add(line);
            }
            Route(line, point);
            JudgeEnd(line, point);
        }

        /// <summary>
        /// 一线多报警点时，根据一端获取距离最远的报警点对应的起点
        /// </summary>
        /// <param name="PerPoint">与起点求距离的端点</param>
        /// <param name="NewAlarmPoint">新增报警点对应线段</param>
        /// <returns></returns>
        private Point GetFurthestPoint(Point PerPoint, AlarmPoint NewAlarmPoint)
        {
            int maxIndex = 0;
            double CurDistance = 0.0;
            List<AlarmPoint> AllAlarmPoint = FootDropLines.FindAll(x => x.FootDropLine.Name == NewAlarmPoint.FootDropLine.Name);
            double maxDistance = Math.Sqrt((AllAlarmPoint[0].Perpendicular.X - PerPoint.X) * (AllAlarmPoint[0].Perpendicular.X - PerPoint.X) + (AllAlarmPoint[0].Perpendicular.Y - PerPoint.Y) * (AllAlarmPoint[0].Perpendicular.Y - PerPoint.Y));
            for (int i = 1; i < AllAlarmPoint.Count; i++)
            {
                CurDistance = Math.Sqrt((AllAlarmPoint[i].Perpendicular.X - PerPoint.X) * (AllAlarmPoint[i].Perpendicular.X - PerPoint.X) + (AllAlarmPoint[i].Perpendicular.Y - PerPoint.Y) * (AllAlarmPoint[i].Perpendicular.Y - PerPoint.Y));
                if (maxDistance < CurDistance)
                {
                    maxDistance = CurDistance;
                    maxIndex = i;
                }
            }
            return AllAlarmPoint[maxIndex].Perpendicular;
        }

        /// <summary>
        /// 多报警点在同一线段上时，找出两端的最后一个点
        /// </summary>
        /// <param name="NewLine">新加的报警点对应的线段 </param>
        /// <returns name="AcmePoint">点的集合，集合中第一个元素是坐标最小的起点，第二个元素是坐标最大的起点</returns>
        private List<Point> GetAcme(Line NewLine)
        {
            List<Point> AcmePoint = new List<Point>();
            Point minPerpendicular, maxPerpendicular;
            List<AlarmPoint> oldAlarmPoints = FootDropLines.FindAll(x => x.FootDropLine.X1 == NewLine.X1 && x.FootDropLine.X2 == NewLine.X2 && x.FootDropLine.Y1 == NewLine.Y1 && x.FootDropLine.Y2 == NewLine.Y2);

            if (oldAlarmPoints.Count > 2)
            {
                int minIndex, maxIndex;
                minIndex = maxIndex = 0;
                if (oldAlarmPoints[0].FootDropLine.X1 == oldAlarmPoints[0].FootDropLine.X2)
                {
                    for (int i = 1; i < oldAlarmPoints.Count; i++)
                    {
                        if (oldAlarmPoints[i].Perpendicular.Y < oldAlarmPoints[minIndex].Perpendicular.Y)
                        {
                            minIndex = i;
                        }
                        if (oldAlarmPoints[i].Perpendicular.Y > oldAlarmPoints[maxIndex].Perpendicular.Y)
                        {
                            maxIndex = i;
                        }

                    }
                    minPerpendicular = oldAlarmPoints[minIndex].Perpendicular;
                    maxPerpendicular = oldAlarmPoints[maxIndex].Perpendicular;
                }
                else
                {
                    for (int i = 1; i < oldAlarmPoints.Count; i++)
                    {
                        if (oldAlarmPoints[i].Perpendicular.X < oldAlarmPoints[minIndex].Perpendicular.X)
                        {
                            minIndex = i;
                        }
                        if (oldAlarmPoints[i].Perpendicular.X > oldAlarmPoints[maxIndex].Perpendicular.X)
                        {
                            maxIndex = i;
                        }

                    }
                    minPerpendicular = oldAlarmPoints[minIndex].Perpendicular;
                    maxPerpendicular = oldAlarmPoints[maxIndex].Perpendicular;
                }
            }
            else
            {

                if (oldAlarmPoints[0].FootDropLine.X1 == oldAlarmPoints[0].FootDropLine.X2)
                {
                    if (oldAlarmPoints[0].Perpendicular.Y < oldAlarmPoints[1].Perpendicular.Y)
                    {
                        minPerpendicular = oldAlarmPoints[0].Perpendicular;
                        maxPerpendicular = oldAlarmPoints[1].Perpendicular;
                    }
                    else
                    {
                        minPerpendicular = oldAlarmPoints[1].Perpendicular;
                        maxPerpendicular = oldAlarmPoints[0].Perpendicular;
                    }
                }
                else
                {
                    if (oldAlarmPoints[0].Perpendicular.X < oldAlarmPoints[1].Perpendicular.X)
                    {
                        minPerpendicular = oldAlarmPoints[0].Perpendicular;
                        maxPerpendicular = oldAlarmPoints[1].Perpendicular;
                    }
                    else
                    {
                        minPerpendicular = oldAlarmPoints[1].Perpendicular;
                        maxPerpendicular = oldAlarmPoints[0].Perpendicular;
                    }
                }

            }

            AcmePoint.Add(minPerpendicular);
            AcmePoint.Add(maxPerpendicular);
            return AcmePoint;
        }

        /// <summary>
        /// 以新增报警点为中心，绘画两边的其中一边的箭头
        /// </summary>
        /// <param name="alarm">新增报警点对应的信息</param>
        /// <param name="point">新增报警点对应的线段两端中其中一个端点</param>
        private void PrintPartArrow(AlarmPoint alarm, List<Point> points, Point point)
        {
            ChaRouteDirection.Clear();
            if (JudgeDirection(alarm.FootDropLine, point))
            {
                Point OtherPoint = new Point();
                AdjoinLine adjoin = new AdjoinLine();
                OtherPoint = GetOtherPoint(alarm.FootDropLine, point);

                if (alarm.FootDropLine.X1 == alarm.FootDropLine.X2)
                {
                    if (point.Y < OtherPoint.Y)
                    {
                        adjoin.alarm = FootDropLines.Find(x => x.Perpendicular == points[0]);
                    }
                    else
                    {
                        adjoin.alarm = FootDropLines.Find(x => x.Perpendicular == points[1]);
                    }
                }
                else
                {
                    if (point.X < OtherPoint.X)
                    {
                        adjoin.alarm = FootDropLines.Find(x => x.Perpendicular == points[0]);
                    }
                    else
                    {
                        adjoin.alarm = FootDropLines.Find(x => x.Perpendicular == points[1]);
                    }
                }
                UpdateLine(point, adjoin, false);
                RouteAgain(adjoin.alarm.FootDropLine, point);

            }
            else
            {
                DeleteArrow(alarm.FootDropLine, point);
            }
        }

        /// <summary>
        /// 新旧报警点相应的线段相邻时线路的变化
        /// </summary>
        /// <param name="adjoinLines">与新报警点对应的线段相邻的旧报警点线段</param>
        /// <param name="Newalarmpoint">新报警点对应的线路起点</param>
        private void AdjacentLinesPrinting(List<AdjoinLine> adjoinLines, AlarmPoint Newalarmpoint)
        {
            List<Image> OldImages = new List<Image>();
            AdjoinLine adjoinLine = new AdjoinLine();
            Point intersection = new Point();
            List<Line> LeftOtherLines = new List<Line>();
            List<Line> RightOtherLines = new List<Line>();
            bool IsNewLeft = false;//是否已经重新绘画起点左侧的箭头

            Point LeftEndPoint = new Point();
            LeftEndPoint.X = Newalarmpoint.FootDropLine.X1;
            LeftEndPoint.Y = Newalarmpoint.FootDropLine.Y1;
            Point RightEndPoint = new Point();
            RightEndPoint.X = Newalarmpoint.FootDropLine.X2;
            RightEndPoint.Y = Newalarmpoint.FootDropLine.Y2;


            for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
            {
                if (LstEscapeLinesCurrentFloor[j] == null)
                {
                    continue;
                }
                else
                {
                    Line otherLine = new Line();
                    otherLine.Name = LstEscapeLinesCurrentFloor[j].Name;
                    otherLine.X1 = LstEscapeLinesCurrentFloor[j].TransformX1;
                    otherLine.X2 = LstEscapeLinesCurrentFloor[j].TransformX2;
                    otherLine.Y1 = LstEscapeLinesCurrentFloor[j].TransformY1;
                    otherLine.Y2 = LstEscapeLinesCurrentFloor[j].TransformY2;
                    if (FootDropLines.FindAll(x => x.FootDropLine.X1 == otherLine.X1 && x.FootDropLine.X2 == otherLine.X2 && x.FootDropLine.Y1 == otherLine.Y1 && x.FootDropLine.Y2 == otherLine.Y2).Count == 0)//找出两边相邻的但不是火灾报警点对应的线段
                    {
                        if ((otherLine.X1 == LeftEndPoint.X && otherLine.Y1 == LeftEndPoint.Y) || (otherLine.X2 == LeftEndPoint.X && otherLine.Y2 == LeftEndPoint.Y))
                        {
                            if (FootDropLines.FindAll(x => x.Perpendicular == LeftEndPoint).Count == 0)
                            {
                                LeftOtherLines.Add(otherLine);
                            }
                        }
                        if ((otherLine.X1 == RightEndPoint.X && otherLine.Y1 == RightEndPoint.Y) || (otherLine.X2 == RightEndPoint.X && otherLine.Y2 == RightEndPoint.Y))
                        {
                            if (FootDropLines.FindAll(x => x.Perpendicular == RightEndPoint).Count == 0)
                            {
                                RightOtherLines.Add(otherLine);
                            }
                        }
                    }
                }
            }

            if (LeftOtherLines.Count == 0 && RightOtherLines.Count == 0)
            {
                Point NextOrigin = new Point();
                NextOrigin.X = Newalarmpoint.FootDropLine.X1;
                NextOrigin.Y = Newalarmpoint.FootDropLine.Y1;
                if (Ends.Contains(NextOrigin))
                {
                    adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                    adjoinLine.AdjacentPoint = LeftEndPoint;
                    _context.DeleteOldImage(adjoinLine.alarm);
                    PrintedLines.Add(adjoinLine.alarm.FootDropLine);
                }
                else
                {
                    AlreadyDeleLine.Clear();
                    DeleteArrow(Newalarmpoint.FootDropLine, NextOrigin);
                }

                NextOrigin.X = Newalarmpoint.FootDropLine.X2;
                NextOrigin.Y = Newalarmpoint.FootDropLine.Y2;
                if (Ends.Contains(NextOrigin))
                {
                    adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                    adjoinLine.AdjacentPoint = LeftEndPoint;
                    _context.DeleteOldImage(adjoinLine.alarm);
                    PrintedLines.Add(adjoinLine.alarm.FootDropLine);
                }
                else
                {
                    AlreadyDeleLine.Clear();
                    DeleteArrow(Newalarmpoint.FootDropLine, NextOrigin);
                }
            }
            else
            {
                //X1、Y1一侧
                if (Ends.Contains(LeftEndPoint))//判断新增报警点左侧端点是否是出口
                {
                    IsNewLeft = true;
                    adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                    adjoinLine.AdjacentPoint = LeftEndPoint;
                    _context.DeleteOldImage(adjoinLine.alarm);
                    UpdateLine(LeftEndPoint, adjoinLine, false);
                }
                else
                {
                    if (LeftOtherLines.Count == 0)
                    {
                        if (FootDropLines.FindAll(x => x.Perpendicular == LeftEndPoint).Count == 0)
                        {
                            #region 一线多点时选出适合的报警点
                            Point MaxPoint = new Point();
                            for (int i = 0; i < adjoinLines.Count; i++)
                            {
                                if (adjoinLines.FindAll(x => x.alarm.FootDropLine == adjoinLines[i].alarm.FootDropLine).Count > 1)
                                {
                                    MaxPoint = GetFurthestPoint(adjoinLines[i].AdjacentPoint, adjoinLines[i].alarm);
                                    for (int j = 0; j < adjoinLines.Count; j++)
                                    {
                                        if (adjoinLines[j].alarm.FootDropLine == adjoinLines[i].alarm.FootDropLine && adjoinLines[j].alarm.Perpendicular != MaxPoint)
                                        {
                                            adjoinLines.RemoveAt(j);
                                            j--;
                                        }
                                    }
                                    i = -1;
                                }
                            }
                            #endregion

                            for (int i = 0; i < adjoinLines.Count; i++)
                            {
                                if (adjoinLines[i].AdjacentPoint.X == Newalarmpoint.FootDropLine.X1 && adjoinLines[i].AdjacentPoint.Y == Newalarmpoint.FootDropLine.Y1)
                                {
                                    if (IsExistArrow(adjoinLines[i]))
                                    {
                                        OldImages.Clear();
                                        _context.DeleteOldImage(adjoinLines[i].alarm);
                                        UpdateLine(adjoinLines[i].AdjacentPoint, adjoinLines[i], true);
                                    }
                                    else
                                    {
                                        _context.DeleteOldImage(adjoinLines[i].alarm, adjoinLines[i].AdjacentPoint);
                                    }
                                }
                            }
                            IsNewLeft = true;
                            Point point = new Point();
                            point.X = Newalarmpoint.FootDropLine.X1;
                            point.Y = Newalarmpoint.FootDropLine.Y1;
                            adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                            adjoinLine.AdjacentPoint = point;
                            _context.DeleteOldImage(adjoinLine.alarm);
                            UpdateLine(point, adjoinLine, true);
                        }
                        else
                        {
                            DeleteArrow(Newalarmpoint.FootDropLine, LeftEndPoint);
                        }

                    }
                    else
                    {
                        bool IsInterlinked = false;
                        intersection.X = Newalarmpoint.FootDropLine.X1;
                        intersection.Y = Newalarmpoint.FootDropLine.Y1;
                        for (int i = 0; i < LeftOtherLines.Count; i++)
                        {
                            for (int j = 0; j < PrintedLines.Count; j++)
                            {
                                if (PrintedLines[j] == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (PrintedLines[j].X1 == LeftOtherLines[i].X1 && PrintedLines[j].X2 == LeftOtherLines[i].X2 && PrintedLines[j].Y1 == LeftOtherLines[i].Y1 && PrintedLines[j].Y2 == LeftOtherLines[i].Y2)
                                    {
                                        if ((Point)PrintedLines[j].Tag == intersection)
                                        {
                                            IsInterlinked = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (IsInterlinked)
                            {
                                break;
                            }
                        }
                        if (IsInterlinked)
                        {
                            IsNewLeft = true;
                            Point point = new Point();
                            point.X = Newalarmpoint.FootDropLine.X2;
                            point.Y = Newalarmpoint.FootDropLine.Y2;
                            adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                            adjoinLine.AdjacentPoint = point;
                            _context.DeleteOldImage(adjoinLine.alarm);
                            UpdateLine(point, adjoinLine, true);
                        }//左侧存在相通的线路
                        //左侧不存在相通的线路
                        else
                        {
                            if (LeftOtherLines.Count == 1)
                            {
                                ChaRouteDirection.Clear();
                                Point point = new Point();
                                point.X = Newalarmpoint.FootDropLine.X1;
                                point.Y = Newalarmpoint.FootDropLine.Y1;
                                if (JudgeDirection(LeftOtherLines[0], (Point)LeftOtherLines[0].Tag))
                                {
                                    IsNewLeft = true;
                                    adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                                    adjoinLine.AdjacentPoint = point;
                                    UpdateLine(point, adjoinLine, false);
                                    //UpdateLine(point, adjoinLine, true);
                                    RouteAgain(Newalarmpoint.FootDropLine, intersection);
                                }
                                else
                                {
                                    DeleteArrow(Newalarmpoint.FootDropLine, point);
                                }
                            }
                            if (LeftOtherLines.Count > 1)
                            {

                                if (LeftOtherLines.FindAll(x => (Point)x.Tag == intersection).Count == 0)
                                {
                                    int TrueIndex = 0;
                                    bool IsExist = false;
                                    for (int i = 0; i < LeftOtherLines.Count; i++)
                                    {
                                        Point FinPoint = (Point)LeftOtherLines[i].Tag;
                                        ChaRouteDirection.Clear();
                                        IsExist = JudgeDirection(LeftOtherLines[i], FinPoint);
                                        if (IsExist)
                                        {
                                            TrueIndex = i;
                                            break;
                                        }
                                    }
                                    if (IsExist)
                                    {
                                        RouteAgain(LeftOtherLines[TrueIndex], intersection);
                                    }
                                    else
                                    {
                                        DeleteArrow(Newalarmpoint.FootDropLine, intersection);
                                    }
                                }
                            }
                        }
                    }
                }
                #region
                //X2、Y2一侧   
                if (Ends.Contains(RightEndPoint))
                {
                    adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                    adjoinLine.AdjacentPoint = RightEndPoint;
                    if (!IsNewLeft)
                    {
                        _context.DeleteOldImage(adjoinLine.alarm);
                    }
                    UpdateLine(RightEndPoint, adjoinLine, false);
                }
                else
                {
                    if (RightOtherLines.Count == 0)
                    {
                        #region 一线多点时选出适合的报警点
                        Point MaxPoint = new Point();
                        for (int i = 0; i < adjoinLines.Count; i++)
                        {
                            if (adjoinLines.FindAll(x => x.alarm.FootDropLine == adjoinLines[i].alarm.FootDropLine).Count > 1)
                            {
                                MaxPoint = GetFurthestPoint(adjoinLines[i].AdjacentPoint, adjoinLines[i].alarm);
                                for (int j = 0; j < adjoinLines.Count; j++)
                                {
                                    if (adjoinLines[j].alarm.FootDropLine == adjoinLines[i].alarm.FootDropLine && adjoinLines[j].alarm.Perpendicular != MaxPoint)
                                    {
                                        adjoinLines.RemoveAt(j);
                                        j--;
                                    }
                                }
                                i = -1;
                            }
                        }
                        #endregion

                        for (int i = 0; i < adjoinLines.Count; i++)
                        {
                            if (adjoinLines[i].AdjacentPoint.X == Newalarmpoint.FootDropLine.X2 && adjoinLines[i].AdjacentPoint.Y == Newalarmpoint.FootDropLine.Y2)
                            {
                                if (IsExistArrow(adjoinLines[i]))
                                {
                                    OldImages.Clear();
                                    _context.DeleteOldImage(adjoinLines[i].alarm);
                                    UpdateLine(adjoinLines[i].AdjacentPoint, adjoinLines[i], true);
                                }
                                else
                                {
                                    _context.DeleteOldImage(adjoinLines[i].alarm, adjoinLines[i].AdjacentPoint);
                                }
                            }
                        }
                        Point point = new Point();
                        point.X = Newalarmpoint.FootDropLine.X2;
                        point.Y = Newalarmpoint.FootDropLine.Y2;
                        adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                        adjoinLine.AdjacentPoint = point;
                        _context.DeleteOldImage(adjoinLine.alarm);
                        UpdateLine(point, adjoinLine, true);

                    }
                    else
                    {
                        bool IsInterlinked = false;
                        intersection.X = Newalarmpoint.FootDropLine.X2;
                        intersection.Y = Newalarmpoint.FootDropLine.Y2;

                        ChaRouteDirection.Clear();
                        IsInterlinked = JudgeDirection(Newalarmpoint.FootDropLine, intersection);
                        if (IsInterlinked)
                        {
                            Point point = new Point();
                            point.X = Newalarmpoint.FootDropLine.X1;
                            point.Y = Newalarmpoint.FootDropLine.Y1;
                            adjoinLine.alarm = FootDropLines.Find(x => x.FootDropLine == Newalarmpoint.FootDropLine);
                            adjoinLine.AdjacentPoint = point;
                            if (FootDropLines.FindAll(x => x.Perpendicular == point).Count != 0)
                            {
                                Route(adjoinLine.alarm.FootDropLine, point);
                            }
                            else
                            {
                                UpdateLine(point, adjoinLine, true);
                            }

                            RouteAgain(adjoinLine.alarm.FootDropLine, intersection);
                        }
                        else
                        {
                            DeleteArrow(adjoinLine.alarm.FootDropLine, intersection);
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// 判断到相关路线不可行后删除该路线上所有箭头
        /// </summary>
        /// <param name="line">被判断的起初线段</param>
        /// <param name="FinPoint">不可行的线路的起点</param>
        private void DeleteArrow(Line line, Point FinPoint)
        {
            if (!Ends.Contains(FinPoint))
            {
                AlarmPoint alarmpoint = new AlarmPoint();
                alarmpoint.FootDropLine = new Line();
                alarmpoint.Perpendicular = new Point();

                List<nextLines> FrontLines = new List<nextLines>();
                //找出Line的前面节点的路线
                for (int i = 0; i < LstEscapeLinesCurrentFloor.Count; i++)
                {
                    if (LstEscapeLinesCurrentFloor[i] == null)
                    {
                        continue;
                    }
                    else
                    {
                        Line FrontLine = new Line();
                        FrontLine.Name = LstEscapeLinesCurrentFloor[i].Name;
                        FrontLine.X1 = LstEscapeLinesCurrentFloor[i].TransformX1;
                        FrontLine.X2 = LstEscapeLinesCurrentFloor[i].TransformX2;
                        FrontLine.Y1 = LstEscapeLinesCurrentFloor[i].TransformY1;
                        FrontLine.Y2 = LstEscapeLinesCurrentFloor[i].TransformY2;
                        if (((FrontLine.X1 == FinPoint.X && FrontLine.Y1 == FinPoint.Y) || (FrontLine.X2 == FinPoint.X && FrontLine.Y2 == FinPoint.Y)) && (FrontLine.X1 != line.X1 || FrontLine.X2 != line.X2 || FrontLine.Y1 != line.Y1 || FrontLine.Y2 != line.Y2) && AlreadyDeleLine.FindAll(x => x.X1 == FrontLine.X1 && x.X2 == FrontLine.X2 && x.Y1 == FrontLine.Y1 && x.Y2 == FrontLine.Y2).Count == 0)
                        {
                            nextLines frontLine = new nextLines();
                            frontLine.nextline = LstEscapeLinesCurrentFloor[i];
                            frontLine.StartingPoint = FinPoint;
                            FrontLines.Add(frontLine);
                        }
                    }
                }
                for (int i = 0; i < FrontLines.Count; i++)
                {
                    if (AlreadyDeleLine.FindAll(x => x.X1 == FrontLines[i].nextline.TransformX1 && x.X2 == FrontLines[i].nextline.TransformX2 && x.Y1 == FrontLines[i].nextline.TransformY1 && x.Y2 == FrontLines[i].nextline.TransformY2).Count > 0)
                    {
                        continue;
                    }
                    List<AlarmPoint> GivenFootDropLines = FootDropLines.FindAll(x => x.FootDropLine.X1 == FrontLines[i].nextline.TransformX1 && x.FootDropLine.X2 == FrontLines[i].nextline.TransformX2 && x.FootDropLine.Y1 == FrontLines[i].nextline.TransformY1 && x.FootDropLine.Y2 == FrontLines[i].nextline.TransformY2);
                    FinPoint = FrontLines[i].StartingPoint;
                    if (GivenFootDropLines.Count == 0)
                    {
                        alarmpoint.FootDropLine.X1 = FrontLines[i].nextline.TransformX1;
                        alarmpoint.FootDropLine.X2 = FrontLines[i].nextline.TransformX2;
                        alarmpoint.FootDropLine.Y1 = FrontLines[i].nextline.TransformY1;
                        alarmpoint.FootDropLine.Y2 = FrontLines[i].nextline.TransformY2;
                        _context.DeleteOldImage(alarmpoint);
                        AlreadyDeleLine.Add(alarmpoint.FootDropLine);
                        if (PrintedLines.Find(x => x.Name == FrontLines[i].nextline.Name) != null)
                        {
                            PrintedLines.Find(x => x.Name == FrontLines[i].nextline.Name).Tag = null;
                        }
                        FinPoint = GetOtherPoint(alarmpoint.FootDropLine, FinPoint);

                        DeleteArrow(alarmpoint.FootDropLine, FinPoint);
                        //AlreadyDeleLine.Add(FrontLines[i].nextline);
                    }
                    else
                    {
                        #region 找出离交点最近的报警点起点
                        Point LatelyPoint = new Point();
                        LatelyPoint = GivenFootDropLines[0].Perpendicular;
                        //线段垂直时
                        if (GivenFootDropLines[0].FootDropLine.X1 == GivenFootDropLines[0].FootDropLine.X2)
                        {
                            if (GivenFootDropLines[0].Perpendicular.Y < FinPoint.Y)
                            {
                                for (int j = 0; j < GivenFootDropLines.Count; j++)
                                {
                                    if (GivenFootDropLines[j].Perpendicular.Y > LatelyPoint.Y)
                                    {
                                        LatelyPoint = GivenFootDropLines[j].Perpendicular;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < GivenFootDropLines.Count; j++)
                                {
                                    if (GivenFootDropLines[j].Perpendicular.Y < LatelyPoint.Y)
                                    {
                                        LatelyPoint = GivenFootDropLines[j].Perpendicular;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (GivenFootDropLines[0].Perpendicular.X < FinPoint.X)
                            {
                                for (int j = 0; j < GivenFootDropLines.Count; j++)
                                {
                                    if (GivenFootDropLines[j].Perpendicular.X > LatelyPoint.X)
                                    {
                                        LatelyPoint = GivenFootDropLines[j].Perpendicular;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < GivenFootDropLines.Count; j++)
                                {
                                    if (GivenFootDropLines[j].Perpendicular.X < LatelyPoint.X)
                                    {
                                        LatelyPoint = GivenFootDropLines[j].Perpendicular;
                                    }
                                }
                            }
                        }
                        #endregion
                        alarmpoint = FootDropLines.Find(x => x.Perpendicular == LatelyPoint);
                        _context.DeleteOldImage(alarmpoint, FinPoint);

                        Point OtherSide = GetOtherPoint(GivenFootDropLines[0].FootDropLine, FinPoint);

                        List<Line> adjLines = new List<Line>();
                        for (int j = 0; j < LstEscapeLinesCurrentFloor.Count; j++)
                        {
                            if (LstEscapeLinesCurrentFloor[j] == null)
                            {
                                continue;
                            }
                            else
                            {
                                Line adjLine = new Line();
                                adjLine.Name = LstEscapeLinesCurrentFloor[j].Name;
                                adjLine.X1 = LstEscapeLinesCurrentFloor[j].TransformX1;
                                adjLine.X2 = LstEscapeLinesCurrentFloor[j].TransformX2;
                                adjLine.Y1 = LstEscapeLinesCurrentFloor[j].TransformY1;
                                adjLine.Y2 = LstEscapeLinesCurrentFloor[j].TransformY2;

                                if ((adjLine.X1 != GivenFootDropLines[0].FootDropLine.X1 || adjLine.X2 != GivenFootDropLines[0].FootDropLine.X2 || adjLine.Y1 != GivenFootDropLines[0].FootDropLine.Y1 || adjLine.Y2 != GivenFootDropLines[0].FootDropLine.Y2) && ((adjLine.X1 == OtherSide.X && adjLine.Y1 == OtherSide.Y) || (adjLine.X2 == OtherSide.X && adjLine.Y2 == OtherSide.Y)) && FootDropLines.FindAll(x => x.FootDropLine.X1 == adjLine.X1 && x.FootDropLine.X2 == adjLine.X2 && x.FootDropLine.Y1 == adjLine.Y1 && x.FootDropLine.Y2 == adjLine.Y2).Count == 0)
                                {
                                    adjLines.Add(adjLine);
                                }
                            }
                        }

                        bool IsExistArrow = false;
                        if (GivenFootDropLines[0].FootDropLine.X1 == GivenFootDropLines[0].FootDropLine.X2)
                        {
                            if (LatelyPoint.Y < OtherSide.Y)
                            {
                                for (int j = 0; j < _context.cvsMainWindow.Children.Count; j++)
                                {
                                    Image AppointImage = _context.cvsMainWindow.Children[j] as Image;
                                    if (AppointImage == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (AppointImage.Tag is Line && (AppointImage.Tag as Line).X1 == GivenFootDropLines[0].FootDropLine.X1 && (AppointImage.Tag as Line).X2 == GivenFootDropLines[0].FootDropLine.X2 && (AppointImage.Tag as Line).Y1 == GivenFootDropLines[0].FootDropLine.Y1 && (AppointImage.Tag as Line).Y2 == GivenFootDropLines[0].FootDropLine.Y2 && AppointImage.Margin.Top + 10 > LatelyPoint.Y)
                                        {
                                            IsExistArrow = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < _context.cvsMainWindow.Children.Count; j++)
                                {
                                    Image AppointImage = _context.cvsMainWindow.Children[j] as Image;
                                    if (AppointImage == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (AppointImage.Tag is Line && (AppointImage.Tag as Line).X1 == GivenFootDropLines[0].FootDropLine.X1 && (AppointImage.Tag as Line).X2 == GivenFootDropLines[0].FootDropLine.X2 && (AppointImage.Tag as Line).Y1 == GivenFootDropLines[0].FootDropLine.Y1 && (AppointImage.Tag as Line).Y2 == GivenFootDropLines[0].FootDropLine.Y2 && AppointImage.Margin.Top + 10 < LatelyPoint.Y)
                                        {
                                            IsExistArrow = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (LatelyPoint.X < OtherSide.X)
                            {
                                for (int j = 0; j < _context.cvsMainWindow.Children.Count; j++)
                                {
                                    Image AppointImage = _context.cvsMainWindow.Children[j] as Image;
                                    if (AppointImage == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (AppointImage.Tag is Line && (AppointImage.Tag as Line).X1 == GivenFootDropLines[0].FootDropLine.X1 && (AppointImage.Tag as Line).X2 == GivenFootDropLines[0].FootDropLine.X2 && (AppointImage.Tag as Line).Y1 == GivenFootDropLines[0].FootDropLine.Y1 && (AppointImage.Tag as Line).Y2 == GivenFootDropLines[0].FootDropLine.Y2 && AppointImage.Margin.Left + 12 > LatelyPoint.X)
                                        {
                                            IsExistArrow = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < _context.cvsMainWindow.Children.Count; j++)
                                {
                                    Image AppointImage = _context.cvsMainWindow.Children[j] as Image;
                                    if (AppointImage == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (AppointImage.Tag is Line && (AppointImage.Tag as Line).X1 == GivenFootDropLines[0].FootDropLine.X1 && (AppointImage.Tag as Line).X2 == GivenFootDropLines[0].FootDropLine.X2 && (AppointImage.Tag as Line).Y1 == GivenFootDropLines[0].FootDropLine.Y1 && (AppointImage.Tag as Line).Y2 == GivenFootDropLines[0].FootDropLine.Y2 && AppointImage.Margin.Left + 12 < LatelyPoint.X)
                                        {
                                            IsExistArrow = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (adjLines.Count == 0)
                        {
                            AlreadyDeleLine.Add(alarmpoint.FootDropLine);
                        }
                        else
                        {
                            if (!IsExistArrow)
                            {
                                AlreadyDeleLine.Add(alarmpoint.FootDropLine);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断指定线段上报警点与指定端点间是否存在箭头
        /// </summary>
        /// <param name="adjoinLine">需要判断的线段信息</param>
        /// <returns></returns>
        private bool IsExistArrow(AdjoinLine adjoinLine)
        {
            bool IsExist = false;
            //当相邻的线段垂直时
            if (adjoinLine.alarm.FootDropLine.X1 == adjoinLine.alarm.FootDropLine.X2)
            {
                for (int j = 0; j < _context.cvsMainWindow.Children.Count; j++)
                {
                    Image OldImage = _context.cvsMainWindow.Children[j] as Image;
                    if (OldImage == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (OldImage.Tag is Line && (OldImage.Tag as Line).X1 == adjoinLine.alarm.FootDropLine.X1 && (OldImage.Tag as Line).X2 == adjoinLine.alarm.FootDropLine.X2 && (OldImage.Tag as Line).Y1 == adjoinLine.alarm.FootDropLine.Y1 && (OldImage.Tag as Line).Y2 == adjoinLine.alarm.FootDropLine.Y2)
                        {
                            if (adjoinLine.AdjacentPoint.Y < adjoinLine.alarm.Perpendicular.Y)
                            {
                                if (OldImage.Margin.Top + 10 > adjoinLine.alarm.Perpendicular.Y)
                                {
                                    IsExist = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (OldImage.Margin.Top + 10 < adjoinLine.alarm.Perpendicular.Y)
                                {
                                    IsExist = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < _context.cvsMainWindow.Children.Count; j++)
                {
                    Image OldImage = _context.cvsMainWindow.Children[j] as Image;
                    if (OldImage == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (OldImage.Tag is Line && (OldImage.Tag as Line).X1 == adjoinLine.alarm.FootDropLine.X1 && (OldImage.Tag as Line).X2 == adjoinLine.alarm.FootDropLine.X2 && (OldImage.Tag as Line).Y1 == adjoinLine.alarm.FootDropLine.Y1 && (OldImage.Tag as Line).Y2 == adjoinLine.alarm.FootDropLine.Y2)
                        {
                            if (adjoinLine.AdjacentPoint.X < adjoinLine.alarm.Perpendicular.X)
                            {
                                if (OldImage.Margin.Left + 12 > adjoinLine.alarm.Perpendicular.X)
                                {
                                    IsExist = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (OldImage.Margin.Left + 12 < adjoinLine.alarm.Perpendicular.X)
                                {
                                    IsExist = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return IsExist;
        }
    }
}
