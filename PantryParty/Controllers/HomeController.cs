﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PantryParty.Models;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace PantryParty.Controllers
{
    public partial class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [Authorize]
        public ActionResult Welcome()
        {
            return View();
        }

        // displays recipes based on ingredients and saves ingredients to database
        [Authorize] //you're only allowed here if you're logged in
        public ActionResult FridgeItems(string input, string UserID)
        {
            // try
            // {
            if (Regex.IsMatch(input, @"^([A-Za-z\s]{1,})$"))
            {
                Ingredient.EditIngredients(input, UserID);
            }
            else if (Regex.IsMatch(input, @"^([A-Za-z\s\,]{1,})$"))
            {
                string[] IngredientArray = input.Split(',');
                Ingredient.EditIngredients(IngredientArray, UserID);
                input = input.Replace(",", "%2C");
            }
            //else
            //{
            //    return View("../Shared/Error");
            //}

            // Gets list of recipes based on ingredients input
            HttpWebRequest request = WebRequest.CreateHttp("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?fillIngredients=false&ingredients=" + input + "&limitLicense=false&number=2&ranking=1");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            string Header = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Header"];
            string APIkey = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Key"];
            request.Headers.Add(Header, APIkey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader dataStream = new StreamReader(response.GetResponseStream());
                string jSonData = dataStream.ReadToEnd();
                JArray recipes = JArray.Parse(jSonData);
                ViewBag.Data = recipes;
                dataStream.Close();
                return DisplayRecipes(recipes, UserID);
            }
            else // if we have something wrong
            {
                return View("../Shared/Error");
            }
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        //  [Authorize]
        public ActionResult DisplayRecipes(JArray recipes, string UserID)
        {
            //try
            //{
            List<Recipe> RecipeList = new List<Recipe>();
            for (int i = 0; i < recipes.Count; i++)
            {
                // gets specific recipe information
                HttpWebRequest request = WebRequest.CreateHttp("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/" + recipes[i]["id"] + "/information");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                string Header = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Header"];
                string APIkey = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Key"];
                request.Headers.Add(Header, APIkey);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string output = reader.ReadToEnd();
                    JObject jParser = JObject.Parse(output);
                    reader.Close();
                    
                    // Take information from JObject and create a new Recipe object
                    Recipe ToAdd = Recipe.Parse(jParser);
                    RecipeList.Add(ToAdd);
                    Recipe.SaveRecipes(ToAdd, UserID);
                    RecipeIngredient.SaveNewRecipeIngredient(jParser, ToAdd);
                }
            }
            ViewBag.RecipeInfo = RecipeList;
            return View("ShowResults");
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        public ActionResult CompareMissingIngredients(string ToCompare, string UserID)
        {
            UserRecipe.SaveRecipe(ToCompare, UserID);
            pantrypartyEntities ORM = new pantrypartyEntities();
            List<Ingredient> RecipesIngredientsList = new List<Ingredient>();
            List<Ingredient> MyIngredients = new List<Ingredient>();

            // Creates list of RecipeIngredient objects and initializes RecipeIngredientsList with all matching values
            List<RecipeIngredient> ChangeToRecipesIng = ORM.RecipeIngredients.Where(x => x.RecipeID == ToCompare).ToList();
            foreach (RecipeIngredient x in ChangeToRecipesIng)
            {
                if (!RecipesIngredientsList.Contains(ORM.Ingredients.Find(x.IngredientID)))
                {
                    RecipesIngredientsList.Add(ORM.Ingredients.Find(x.IngredientID));
                }
            }

            // Creates list of UserIngredient objects and initializes UserIngredientList with all matching values
            List<UserIngredient> ChangeToUserIngredients = ORM.UserIngredients.Where(x => x.UserID == UserID).ToList();
            foreach (UserIngredient x in ChangeToUserIngredients)
            {
                if (!MyIngredients.Contains(ORM.Ingredients.Find(x.IngredientID)))
                {
                    MyIngredients.Add(ORM.Ingredients.Find(x.IngredientID));
                }
            }

            // Edits recipe's ingredients list to contain only missing ingredients
            foreach (Ingredient x in MyIngredients)
            {
                if (RecipesIngredientsList.Contains(x))
                {
                    RecipesIngredientsList.Remove(x);
                }
            }

            // Creates list of Users with any/all of your missing ingredients
            List<AspNetUser> CheckNearby = UserIngredient.FindUsersWith(RecipesIngredientsList);

            // Sends list of nearby users with your missing ingredients to page

            List<AspNetUser> NearbyUsers = AspNetUser.FindNearbyUsers(CheckNearby, UserID);
            ViewBag.NearbyUsers = NearbyUsers;

            ViewBag.CurrentUserLatLong = Geocode(UserID);
            ViewBag.LatLongArray = Geocode(NearbyUsers, UserID);

            // Sends list of your missing ingredients to page
            // ViewBag.MissingIngredients = RecipesIngredientsList;

            List<UserIngredient> Test = new List<UserIngredient>();
            foreach (Ingredient item in RecipesIngredientsList)
            {
                Test.AddRange(ORM.UserIngredients.Where(x => x.IngredientID == item.Name));
            }
            ViewBag.UserIngredients = Test.Distinct().ToList();

            string APIkey = System.Configuration.ConfigurationManager.AppSettings["Google Marker API KEY"];
            ViewData["APIkey"] = APIkey;

            return View("NearbyUsers");
        }

        // Finds latitude and longitude of the logged in user
        public static LatLong Geocode(string CurrentUserID)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            AspNetUser CurrentUser = ORM.AspNetUsers.Find(CurrentUserID);
            LatLong ToReturn = new LatLong();
            string googleplus = Plus(CurrentUser.Address, CurrentUser.City, CurrentUser.State);
            string APIkey = System.Configuration.ConfigurationManager.AppSettings["Google Geocode API KEY"];

            HttpWebRequest request = WebRequest.CreateHttp($"https://maps.googleapis.com/maps/api/geocode/json?address={googleplus}&key={APIkey}");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader rd = new StreamReader(response.GetResponseStream());
                string output = rd.ReadToEnd();
                JObject Jparser = JObject.Parse(output);

                ToReturn.Lat = Jparser["results"][0]["geometry"]["location"]["lat"].ToString();
                ToReturn.Long = Jparser["results"][0]["geometry"]["location"]["lng"].ToString();
                return ToReturn;
            }
            return ToReturn;
        }

        // Finds the latitude and longitude of users with your missing ingredients
        public static LatLong[] Geocode(List<AspNetUser> NearByUsers, string UserID)
        {
            LatLong[] ToReturn = new LatLong[NearByUsers.Count()];

            int i = 0;
            foreach (AspNetUser Person in NearByUsers)
            {
                ToReturn[i] = new LatLong();
                if (Person.ID == UserID)
                {
                    continue;
                }
                string googleplus = Plus(Person.Address, Person.City, Person.State);
                string APIkey = System.Configuration.ConfigurationManager.AppSettings["Google Geocode API KEY"];

                HttpWebRequest request = WebRequest.CreateHttp($"https://maps.googleapis.com/maps/api/geocode/json?address={googleplus}&key={APIkey}");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader rd = new StreamReader(response.GetResponseStream());
                    string output = rd.ReadToEnd();
                    JObject Jparser = JObject.Parse(output);

                    ToReturn[i].Lat = Jparser["results"][0]["geometry"]["location"]["lat"].ToString();
                    ToReturn[i].Long = Jparser["results"][0]["geometry"]["location"]["lng"].ToString();
                    i++;
                }
            }
            return ToReturn;
        }

        public static string Plus(string street, string city, string state)
        {
            street = street.Replace(" ", "+");
            city = city.Replace(" ", "+");
            return street + ",+" + city + ",+" + state;
        }

        // This method may be unnecessary
        public static void FindRecipeInDB(Recipe Selected)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            if (ORM.Recipes.Find(Selected.ID) == null)
            {
                ORM.Recipes.Add(Selected);
                ORM.SaveChanges();
            }
        }

        public ActionResult FindNearbyUsersButton()
        {
            return View();
        }

        // Finds all ingredients that the logged in user has
        public ActionResult EditIngred(string UserID)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            List<UserIngredient> EditThisList = ORM.UserIngredients.Where(x => x.UserID == UserID).ToList();
            List<Ingredient> ToSend = new List<Ingredient>();
            foreach (UserIngredient x in EditThisList)
            {
                ToSend.Add(ORM.Ingredients.Find(x.IngredientID));
            }
            ViewBag.UsersListOfIngred = ToSend;
            return View("EditIngred");
        }

        // Deletes an ingredient with the selected name from the DB and sends back to the Delete view
        public ActionResult Delete(string CurrentUser, string ItemToDelete)
        {
            //try
            //{
            pantrypartyEntities ORM = new pantrypartyEntities();
            ORM.UserIngredients.RemoveRange(ORM.UserIngredients.Where(x => (x.UserID == CurrentUser && x.IngredientID == ItemToDelete)));
            ORM.SaveChanges();
            return EditIngred(CurrentUser);
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        // sends to Edit User Info view
        // if this doesn't work, there's something seriously wrong
        public ActionResult UpdateProfile(string UserID)
        {
            try
            {
                pantrypartyEntities ORM = new pantrypartyEntities();
                AspNetUser ToBeUpdated = ORM.AspNetUsers.Find(UserID);
                ViewBag.UpdateProf = ToBeUpdated;

                return View();
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }
        }

        //SAVED EDIT PROFILE
        public ActionResult SaveProfChanges(AspNetUser NUser)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View("../Shared/Error");
            //}
            pantrypartyEntities ORM = new pantrypartyEntities();
            AspNetUser CurrentUser = ORM.AspNetUsers.Find(NUser.ID);

            CurrentUser.FirstName = NUser.FirstName;
            CurrentUser.LastName = NUser.LastName;
            CurrentUser.PhoneNumber = NUser.PhoneNumber;
            CurrentUser.Address = NUser.Address;
            CurrentUser.City = NUser.City;
            CurrentUser.State = NUser.State;
            CurrentUser.Zipcode = NUser.Zipcode;
            CurrentUser.EmailConfirmed = NUser.EmailConfirmed;

            ORM.Entry(ORM.AspNetUsers.Find(NUser.ID)).CurrentValues.SetValues(CurrentUser); //finding old object, replacing it with new information

            ORM.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
