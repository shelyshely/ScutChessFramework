using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common;

namespace Switch.Script.Model
{
     //公用函数
    public class CommonFunc
    {
        /// <summary>
        /// （矫正后时间）是否今天
        /// 5点前算昨天
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool isTodayAfterAdjust(DateTime time)
        {
            var nowAdjust = DateTime.Now;
            if (nowAdjust.Hour < 5)
            {
                nowAdjust = nowAdjust.AddDays(-1.0);
            }
            var timeAdjust = time;
            if (timeAdjust != DateTime.MinValue && timeAdjust.Hour < 5)
            {
                timeAdjust = timeAdjust.AddDays(-1.0);
            }

            return timeAdjust.Date == nowAdjust.Date;
        }
        /// <summary>
        /// （矫正后时间）是否本月
        /// 1号5点前算上个月
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool isThisMonthAfterAdjust(DateTime time)
        {
            var nowAdjust = DateTime.Now;
            if (nowAdjust.Day == 1 && nowAdjust.Hour < 5)
            {
                nowAdjust = nowAdjust.AddDays(-1.0);
            }
            var timeAdjust = time;
            if (timeAdjust != DateTime.MinValue && timeAdjust.Day == 1 && timeAdjust.Hour < 5)
            {
                timeAdjust = timeAdjust.AddDays(-1.0);
            }

            return (timeAdjust.Year == nowAdjust.Year && timeAdjust.Month == nowAdjust.Month);
        }

        /// <summary>
        /// 从数组中随机快速取N个记录
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T[] GetQuickRandomArray<T>(T[] array, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            List<T> list = new List<T>();
            if (count > array.Length)
            {
                return list.ToArray();
            }
            List<int> indexList = new List<int>();
            for(var i = 0; i < array.Length; ++i) indexList.Add(i);
            while (list.Count < count)
            {
                int index = RandomUtils.GetRandom(0, indexList.Count);
                list.Add(array[index]);
                indexList.Remove(index);
            }

            return list.ToArray();
        }

        /// <summary>
        /// 从[min,max]中随机取cnt个数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public static List<int> GetQuickRandom(int min, int max, int cnt)
        {
            var list = new List<int>();
            if (min >= max || cnt > max - min + 1)
            {
                return list;
            }
            List<int> tmpList = new List<int>();
            for (var i = min; i <= max; ++i) tmpList.Add(i);
            while (list.Count < cnt)
            {
                int index = RandomUtils.GetRandom(0, tmpList.Count);
                list.Add(tmpList[index]);
                tmpList.Remove(index);
            }

            return list;
        }
    }
}
