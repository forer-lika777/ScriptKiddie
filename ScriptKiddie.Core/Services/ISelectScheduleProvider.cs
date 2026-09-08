using ScriptKiddie.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.Core.Services;

public interface ISelectScheduleProvider
{
    public ObservableCollection<SelectSchedule> SelectSchedules { get; }
    public void Remove(SelectSchedule selectSchedule);
    public Task RemoveRange(IEnumerable<SelectSchedule> selectSchedules);
    public void Remove(int hash);
    public void Add(SelectSchedule schedule);

    /// <summary>
    /// 供外部在修改集合内部对象的属性时调用
    /// </summary>
    public void Update();
}
