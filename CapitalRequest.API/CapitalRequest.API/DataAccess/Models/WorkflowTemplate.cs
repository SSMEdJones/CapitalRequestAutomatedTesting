﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
namespace CapitalRequest.API.DataAccess.Models;

public partial class WorkflowTemplate
{
    public int Id { get; set; }

    public string StepName { get; set; }

    public string StepDescription { get; set; }

    public short StepNumber { get; set; }

    public string Conditional { get; set; }

    public string AdditionalTask { get; set; }

    public DateTime Created { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public string UpdatedBy { get; set; }
}