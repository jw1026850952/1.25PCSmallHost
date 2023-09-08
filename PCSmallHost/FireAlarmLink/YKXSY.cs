using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.FireAlarmLink
{
    public class YKXSY : AbsFireAlarmLink
    {
        public int DealFireAlarmData(byte[] FireAlarmReceiveData,List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            if(FireAlarmReceiveData[0] == 64 && FireAlarmReceiveData[1] == 64 && FireAlarmReceiveData[41] == 35 && FireAlarmReceiveData[42] == 35)//（判断头尾四个数据是否符合协议要求）D0和D1是0X40，D41和D42是0X23
            {
                if(FireAlarmReceiveData[33] == 3 && FireAlarmReceiveData[34] == 0)//D33是0X03和D34是0X00（部件状态是火警）
                {
                    int fireAlarmZoneNumber = 0;//记录符合条件的预案分区
                    int mainBoardCircuit = FireAlarmReceiveData[31];//D31是主板回路
                    int deviceValue = FireAlarmReceiveData[32];//D32是设备地址
                    foreach(FireAlarmPartitionSetInfo infoFireAlarmPartitionSet in LstFireAlarmPartitionSet)
                    {
                        if(infoFireAlarmPartitionSet.MainBoardCircuit == mainBoardCircuit && infoFireAlarmPartitionSet.LowDeviceRange >= deviceValue && infoFireAlarmPartitionSet .HighDeviceRange <= deviceValue)//寻找分区设置数据中主板同一回路并且设备地址范围满足条件的预案分区
                        {
                            fireAlarmZoneNumber = infoFireAlarmPartitionSet.PlanPartition;
                            break;
                        }
                    }
                    if(fireAlarmZoneNumber != 0)
                    {
                        return fireAlarmZoneNumber;
                    }
                    return 0;
                }
                return 0;
            }
            return 0;
        }
    }
}
