using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ScriptKiddie.WinUI.Models;

public class CourseResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; } = -1;

    [JsonPropertyName("rows")]
    public List<CourseItem> Rows { get; set; } = [];
}

public partial class CourseItem : ObservableObject
{
    [JsonPropertyName("kcrwdm")]
    [Display(Name = "课程任务代码")]
    public string? CourseTaskCode { get; set; }

    [JsonPropertyName("pkrs")]
    [Display(Name = "排课人数")]
    public string? PlannedStudentCount { get; set; } // 排课人数

    [JsonPropertyName("jxbdm")]
    [Display(Name = "教学班代码")]
    public string? TeachingClassCode { get; set; }

    [JsonPropertyName("kcptdm")]
    [Display(Name = "课程计划代码")]
    public string? CoursePlanCode { get; set; }

    [JsonPropertyName("xmmc")]
    [Display(Name = "活动描述")]
    public string? ActivityDescription { get; set; }

    [JsonPropertyName("kcdm")]
    [Display(Name = "课程代码")]
    public string? CourseCode { get; set; }

    [JsonPropertyName("kcmc")]
    [Display(Name = "课程名称")]
    public string? CourseName { get; set; }

    [JsonPropertyName("rwdm")]
    [Display(Name = "任务代码")]
    public string? TaskCode { get; set; }

    [JsonPropertyName("xbyqdm")]
    [Display(Name = "选必要求代码")]
    public string? GraduationRequirementCode { get; set; }

    [JsonPropertyName("rs1")]
    [Display(Name = "保留字段1")]
    public string? ReservedField1 { get; set; }

    [JsonPropertyName("rs2")]
    [Display(Name = "保留字段2")]
    public string? ReservedField2 { get; set; }

    [JsonPropertyName("wyfjdm")]
    [Display(Name = "外语附加代码")]
    public string? ForeignLanguageCode { get; set; }

    [JsonPropertyName("kkxqdm")]
    [Display(Name = "开课学期代码")]
    public string? SemesterCode { get; set; }

    [JsonPropertyName("zxs")]
    [Display(Name = "总学时")]
    public string? TotalHours { get; set; }

    [JsonPropertyName("xf")]
    [Display(Name = "学分")]
    public string? Credits { get; set; }

    [JsonPropertyName("kcdlmc")]
    [Display(Name = "课程大类名称")]
    public string? CourseCategoryName { get; set; }

    [JsonPropertyName("kcflmc")]
    [Display(Name = "课程分类名称")]
    public string? CourseTypeName { get; set; }

    [JsonPropertyName("teaxm")]
    [Display(Name = "教师姓名")]
    public string? TeacherName { get; set; }

    [ObservableProperty]
    [JsonPropertyName("jxbrs")]
    [Display(Name = "已选人数")]
    public partial string? SelectedStudentCount { get; set; }

    // 2. 实现源生成器自动为您生成的变动钩子（ partial 方法 ）
    partial void OnSelectedStudentCountChanged(string? value)
    {
        // 防空转换，避免 int.Parse 报错
        if (int.TryParse(value, out int selected) && int.TryParse(PlannedStudentCount, out int planned))
        {
            // 已选人数 >= 计划人数 时标记为已满（或者按你的业务修改逻辑）
            IsFull = selected >= planned;
        }
        else
        {
            IsFull = false;
        }
    }

    // 注意：第7条数据包含额外字段
    [JsonPropertyName("xid")]
    [Display(Name = "学id")]
    public string? ExtraId { get; set; }

    [JsonPropertyName("xsdm")]
    [Display(Name = "学生代码")]
    public string? StudentCode { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    [Display(Name = "已满")]
    public partial bool IsFull { get; set; } = false;

    public override string ToString()
    {
        if (String.IsNullOrWhiteSpace(ActivityDescription))
        {
            return $"[{CourseCode}] {CourseName} - {TeacherName} ({Credits}学分) 限选{PlannedStudentCount}/已选{SelectedStudentCount}";
        }
        else
        {
            return $"[{CourseCode}] {CourseName} [{ActivityDescription}] - {TeacherName} ({Credits}学分) 限选{PlannedStudentCount}/已选{SelectedStudentCount}";
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CourseItem courseItem)
            return false;
        return courseItem.CourseTaskCode == CourseTaskCode && courseItem.CourseName == CourseName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CourseTaskCode, CourseName);
    }

    [JsonIgnore]
    public IEnumerable<PropertyDisplayItem> PropertyDisplayItems
    {
        get
        {
            // Manually list all property field for AOT compability
            yield return new(GetDisplayName(nameof(CourseTaskCode)), CourseTaskCode);
            yield return new(GetDisplayName(nameof(PlannedStudentCount)), PlannedStudentCount);
            yield return new(GetDisplayName(nameof(TeachingClassCode)), TeachingClassCode);
            yield return new(GetDisplayName(nameof(CoursePlanCode)), CoursePlanCode);
            yield return new(GetDisplayName(nameof(ActivityDescription)), ActivityDescription);
            yield return new(GetDisplayName(nameof(CourseCode)), CourseCode);
            yield return new(GetDisplayName(nameof(CourseName)), CourseName);
            yield return new(GetDisplayName(nameof(TaskCode)), TaskCode);
            yield return new(GetDisplayName(nameof(GraduationRequirementCode)), GraduationRequirementCode);
            yield return new(GetDisplayName(nameof(ReservedField1)), ReservedField1);
            yield return new(GetDisplayName(nameof(ReservedField2)), ReservedField2);
            yield return new(GetDisplayName(nameof(ForeignLanguageCode)), ForeignLanguageCode);
            yield return new(GetDisplayName(nameof(SemesterCode)), SemesterCode);
            yield return new(GetDisplayName(nameof(TotalHours)), TotalHours);
            yield return new(GetDisplayName(nameof(Credits)), Credits);
            yield return new(GetDisplayName(nameof(CourseCategoryName)), CourseCategoryName);
            yield return new(GetDisplayName(nameof(CourseTypeName)), CourseTypeName);
            yield return new(GetDisplayName(nameof(TeacherName)), TeacherName);
            yield return new(GetDisplayName(nameof(SelectedStudentCount)), SelectedStudentCount);
            yield return new(GetDisplayName(nameof(IsFull)), IsFull.ToString());
        }
    }

    private static string GetDisplayName(string propertyName)
    {
        // 使用 nameof 确保编译时检查
        var property = typeof(CourseItem).GetProperty(propertyName);
        if (property != null)
        {
            var display = property.GetCustomAttribute<DisplayAttribute>();
            if (display != null && !string.IsNullOrEmpty(display.Name))
                return display.Name;
        }
        return propertyName;
    }
}