//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace PantryParty.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class AspNetUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AspNetUser()
        {
            this.UserIngredients = new HashSet<UserIngredient>();
            this.UserRecipes = new HashSet<UserRecipe>();
        }

        public string ID { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        [Required]
        [RegularExpression(@"^([\d]){10}$")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        [Required]
        [RegularExpression(@"^([A-Za-z0-9]){8,20}$")]
        public string UserName { get; set; }
        [Required]
        [RegularExpression(@"^([A-Za-z]){1,}$")]
        public string FirstName { get; set; }
        [Required]
        [RegularExpression(@"^([A-Za-z]){1,}$")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^([A-Za-z0-9\s]){10,}$")]
        public string Address { get; set; }
        [Required]
        [RegularExpression(@"^([A-Za-z\-]){1,}$")]
        public string City { get; set; }
        [Required]
        [RegularExpression(@"^([A-Z]){2}$")]
        public string State { get; set; }
        [Required]
        [RegularExpression(@"^([\d]){5}$")]
        public string Zipcode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserIngredient> UserIngredients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRecipe> UserRecipes { get; set; }
    }
}