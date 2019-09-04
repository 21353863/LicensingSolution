﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LicensingSolution.Data;
using LicensingSolution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace LicensingSolution.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public SelectList AssociationNameSL { get; set; }

        public class InputModel
        {
            [Display(Name = "Title")]
            public string Title { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Please make sure there are no spaces, special characters & numbers on {0}")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Please make sure there are no spaces, special characters & numbers on {0}")]
            [Display(Name = "Middle Name")]
            public string MiddleName { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Please make sure there are no spaces, special characters & numbers on {0}")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Association")]
            [Required(ErrorMessage = "Please select {0}")]
            public int AssociationId { get; set; }

            [Required(ErrorMessage = "Please select user {0}")]
            [Display(Name = "Role")]
            public string Role { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void PopulateAssociationsDropdownList(object selectedAssociation = null)
        {
            //var associationsQuery = (from d in _context.Associations
            //                            orderby d.Name // Sort by name.
            //                            select d);

            //AssociationNameSL = new SelectList(associationsQuery.AsNoTracking(),
            //            "AssociationId", "Name", selectedAssociation);
        }

        public void OnGet(string returnUrl = null)
        {
            //PopulateAssociationsDropdownList();
            AssociationNameSL = new SelectList(_context.Associations, "AssociationId", "Name");
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                //var userinfo = new UserContact { Title = Input.Title, FirstName = Input.FirstName, LastName = Input.LastName };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (Input.Role == "Owner")
                    {                          
                        await _userManager.AddToRoleAsync(user, "Owner");
                    }
                    if (Input.Role == "AssociationClerk")
                    {
                        await _userManager.AddToRoleAsync(user, "AssociationClerk");
                    }
                    _logger.LogInformation("User added to role.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    StatusMessage = "User registered successfully";
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            PopulateAssociationsDropdownList();
            return Page();
        }
    }
}