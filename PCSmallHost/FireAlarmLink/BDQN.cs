using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.FireAlarmLink
{
    public class BDQN : AbsFireAlarmLink
    {
        public int DealFireAlarmData(byte[] FireAlarmReceiveData,List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            if(FireAlarmReceiveData[0] == 130 && FireAlarmReceiveData[11] == 131)//（判断头尾两个数据是否符合协议要求）D0是0X82 D11是0X83
            {
                //D1到D10中的字节拆成2个半字节加上0X30再发送，先发高字节
                if(FireAlarmReceiveData[1] == 56 && FireAlarmReceiveData[2] == 48)//D1是0X80,拆解再重组后是0X38和0X30（部件状态是火警）
                {
                    int fireAlarmZoneNumber = 0;//记录符合条件的预案分区
                    int mainBoardCircuit = Convert.ToInt32(string.Format("{0}{1}", FireAlarmReceiveData[5] - 48, FireAlarmReceiveData[6] - 48));//D3是回路号
                    int deviceValue = Convert.ToInt32(string.Format("{0}{1}", FireAlarmReceiveData[7] - 48, FireAlarmReceiveData[8] - 48), 16);
                    foreach(FireAlarmPartitionSetInfo infoFireAlarmPartitionSet in LstFireAlarmPartitionSet)
                    {
                        if(infoFireAlarmPartitionSet.MainBoardCircuit == mainBoardCircuit && infoFireAlarmPartitionSet.LowDeviceRange >= deviceValue && infoFireAlarmPartitionSet.HighDeviceRange <= deviceValue)//寻找分区设置数据中主板同一个回路并且设备地址范围满足条件的预案分区
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
