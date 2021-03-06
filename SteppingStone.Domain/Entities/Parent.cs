﻿using SteppingStone.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
{
    public class Parent : Person
    {
        public Parent()
        {
            Students = new List<StudentParent>();
            Added = DateTime.Now;
        }

        [Key]
        public int ParentId { get; set; }

        [Display(Name = "Latest reminder sent")]
        public DateTime? Notified { get; set; }

        [Display(Name = "Next reminder")]
        public DateTime? RemindDate { get; set; }

        [Display(Name = "System reminders sent")]
        public int RemindCount { get; set; }

        public DateTime Added { get; set; }

        public DateTime? Terminated { get; set; }

        public virtual ICollection<StudentParent> Students { get; set; }

        [NotMapped]
        public bool ShouldRemind
        {
            get
            {
                return /*Students.SelectMany(x => x.HasOutstanding());*/ true;
            }
        }

        public bool Matches(string name)
        {
            name = name.ToLower();

            return FirstName.ToLower().Contains(name) || LastName.ToLower().Contains(name);
        }

        public string GetContact()
        {
            return string.IsNullOrEmpty(Mobile) ? PhoneNumber : string.Format("{0} / {1}", PhoneNumber, Mobile);
        }

        [NotMapped]
        public bool HasContact {
            get {
                return !string.IsNullOrEmpty(GetContact());
            }
        }

        [NotMapped]
        public string[] Contacts
        {
            get
            {                
                return string.IsNullOrEmpty(Mobile) ? new string[] { PhoneNumber } : new string[] { PhoneNumber, Mobile };
            }
        }

        [NotMapped]
        public Payment LastPayment
        {
            get
            {
                var students = Students.Count() > 0 ? Students : null;
                var payments = new List<Payment>();

                if(students == null) {
                    return new Payment();
                }

                foreach(var student in students)
                {
                    if(student.Student.Payments.Count() > 0)
                    {
                        payments.AddRange(student.Student.Payments);
                    }
                }

                return payments.Count() > 0 ? payments.OrderByDescending(m => m.Date).First() : new Payment();
            }
        }

        [NotMapped]
        public double Oustanding
        {
            get
            {
                return Students.Count() > 0 ? Students.Select(x => x.Student).Where(m => m.HasOutstanding).Sum(y => y.Outstanding): 0;
            }
        }

        [NotMapped]
        public double CurrentTermPayments
        {
            get
            {
                return Students.Count() > 0 ? Students.Select(x => x.Student).Sum(y => y.StudentTermPaymentsTotal()) : 0;
            }
        }

        [NotMapped]
        public double OverallPayments
        {
            get
            {
                return Students.Count() > 0 ? Students.Select(x => x.Student).Sum(y => y.Total) : 0;
            }
        }

        public string GetStatus()
        {
            if(Oustanding > 0)
            {
                return "Has Debt";
            }
            else if((DateTime.Today - LastPayment.Date).TotalDays > 100)
            {
                return "In-Active";
            }else
            {
                return "Active";
            }           
        }

        public string GetStatusCssClass()
        {
            string status = GetStatus();

            switch (status)
            {
                case "Active":
                    return "success";
                case "Has Debt":
                    return "danger";
                case "In-Active":
                    return "warning";
                default:
                    return "info";
            }
        }

        public string[] GetStudents()
        {
            return Students.Select(x => x.Student.FullName).ToArray();
        }
    }
}
