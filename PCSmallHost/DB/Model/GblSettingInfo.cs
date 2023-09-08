using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// 系统参数实体类
    /// </summary>
    public class GblSettingInfo
    {
        public GblSettingInfo()
		{}
		#region Model
		private int _id;
		private string _key;
		private string _setvalue;
		/// <summary>
		/// 
		/// </summary>
		public int ID
		{
			set{ _id=value;}
			get{return _id;}
		}
		/// <summary>
		/// 键
		/// </summary>
		public string Key
		{
			set{ _key=value;}
			get{return _key;}
		}
		/// <summary>
		/// 值
		/// </summary>
		public string SetValue
		{
			set{ _setvalue=value;}
			get{return _setvalue;}
		}
		#endregion Model
	}    
}
