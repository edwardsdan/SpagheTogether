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
    
    public partial class UserRecipe
    {
        public string UserID { get; set; }
        public string RecipeID { get; set; }
        public int keyvalue { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Recipe Recipe { get; set; }

        public static void SaveRecipe(string RecipeID, string UserID)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            // some if statement

        }
    }
}
