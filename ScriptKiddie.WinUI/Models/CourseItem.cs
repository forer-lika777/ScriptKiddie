using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ScriptKiddie.WinUI.Models;

public class CourseResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; } = -1;

    [JsonPropertyName("rows")]
    public List<CourseItem> Rows { get; set; } = [];
}

public class CourseItem
{
    [JsonPropertyName("kcrwdm")]
    public string? CourseTaskCode { get; set; }

    [JsonPropertyName("pkrs")]
    public string? PlannedStudentCount { get; set; } // 排课人数

    [JsonPropertyName("jxbdm")]
    public string? TeachingClassCode { get; set; }

    [JsonPropertyName("kcptdm")]
    public string? CoursePlanCode { get; set; }

    [JsonPropertyName("xmmc")]
    public string? ActivityDescription { get; set; }

    [JsonPropertyName("kcdm")]
    public string? CourseCode { get; set; }

    [JsonPropertyName("kcmc")]
    public string? CourseName { get; set; }

    [JsonPropertyName("rwdm")]
    public string? TaskCode { get; set; }

    [JsonPropertyName("xbyqdm")]
    public string? GraduationRequirementCode { get; set; }

    [JsonPropertyName("rs1")]
    public string? ReservedField1 { get; set; }

    [JsonPropertyName("rs2")]
    public string? ReservedField2 { get; set; }

    [JsonPropertyName("wyfjdm")]
    public string? ForeignLanguageCode { get; set; }

    [JsonPropertyName("kkxqdm")]
    public string? SemesterCode { get; set; }

    [JsonPropertyName("zxs")]
    public string? TotalHours { get; set; }

    [JsonPropertyName("xf")]
    public string? Credits { get; set; }

    [JsonPropertyName("kcdlmc")]
    public string? CourseCategoryName { get; set; }

    [JsonPropertyName("kcflmc")]
    public string? CourseTypeName { get; set; }

    [JsonPropertyName("teaxm")]
    public string? TeacherName { get; set; }

    [JsonPropertyName("jxbrs")]
    public string? SelectedStudentCount { get; set; } // 已选人数

    // 注意：第7条数据包含额外字段
    [JsonPropertyName("xid")]
    public string? ExtraId { get; set; }

    [JsonPropertyName("xsdm")]
    public string? StudentCode { get; set; }

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
        if (obj is not CourseItem courseItem) return false;
        return courseItem.CourseTaskCode == CourseTaskCode && courseItem.CourseName == CourseName;
    }
}