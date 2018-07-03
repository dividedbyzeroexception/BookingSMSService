using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Timers;
using System.Security.Cryptography;
using System.Data.Entity;
using Client.Struct;

namespace Client
{
    using System;
    using System.Diagnostics;

    using ConsoleApp4;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.OData.Client;
    using Microsoft.OData.Edm;
    using System.Linq;
    using System.Net;
    using System.Configuration;
    using Newtonsoft.Json;

    public class Program
    {
        // See README.MD for instructions on how to get your own values for these two settings.
        // See also https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-scenarios#native-application-to-web-api


        private static string _ApplicationAppId =       ConfigurationManager.AppSettings["ApplicationAppId"];
        private static Uri _ApplicationRedirectUri =    new Uri(ConfigurationManager.AppSettings["ApplicationRedirectUri"]);
        private static AuthenticationResult authenticationResult;
        private static AuthenticationContext authenticationContext;

        private static string _SMSSenderFrom =          ConfigurationManager.AppSettings["SMSSenderFrom"];
        private static string _SMSServiceUserName =     ConfigurationManager.AppSettings["SMSServiceUserName"];
        private static string _SMSServicePassword  =    ConfigurationManager.AppSettings["SMSServicePassword"];
        private static string _BookingBusinessesID =    ConfigurationManager.AppSettings["BookingBusinessesID"];
        private static BookingEntities db;

        private static async Task<string> GetAuthorizationHeader()
        {
            string applicationId = "992c7324-d113-4335-b4bb-13b7d4ace7ed";
            string authority = "https://login.microsoftonline.com/common/";
            Uri redirectUri = new Uri("https://redir/BookingSMSSample");
            AuthenticationContext context = new AuthenticationContext(authority);
            AuthenticationResult result = await context.AcquireTokenAsync("https://graph.microsoft.com", applicationId, redirectUri, new PlatformParameters(PromptBehavior.Auto));
            return result.CreateAuthorizationHeader();
        }

        private static async Task<string> GetBookingAppointments()
        {
            string authHeader = await GetAuthorizationHeader();
            HttpClient graphClient = new HttpClient();
            graphClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            return await graphClient.GetStringAsync("https://graph.microsoft.com/beta/bookingBusinesses/Karrieresenter1@ostfoldfk.no/appointments");
        }

        

        private static Timer callGraphServiceTimer = new Timer(200000);


        private static void HandleTimer(object source, ElapsedEventArgs a)
        {
            //isTimeElapsed = true;
            Console.WriteLine("timerEvent Handled at: {0}", a.SignalTime.ToString());
            authenticationResult = authenticationContext.AcquireTokenSilentAsync(GraphService.ResourceId, _ApplicationAppId).Result;

        }

       

        public static void Main()
        {
            callGraphServiceTimer.Start();
            callGraphServiceTimer.Elapsed +=  (sender, e) =>  HandleTimer(sender, e);
            // string appointments = GetBookingAppointments().GetAwaiter().GetResult();

            //string xclientSecret = "deQQ098_~czsfxQRJYJ37@^";
            //string xclientApplicationAppId = "1c3268d4-84ee-4c88-957b-0e49f77f3098";
            //string xclientApplicationRedirectUri = "https://bookingsmsservice/redirect";

            ViaNettSMS viaNettSMS = new ViaNettSMS(_SMSServiceUserName, _SMSServicePassword);



            try
            {


                // ADAL: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-libraries
                authenticationContext = new AuthenticationContext(GraphService.DefaultAadInstance, TokenCache.DefaultShared);
                /*
                static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant)
                var authenticationContext = new AuthenticationContext()

                */



                //UserPasswordCredential xCred = new UserPasswordCredential(xclientApplicationAppId, xclientSecret);
                //ClientCredential xclientcred = new ClientCredential(xclientApplicationAppId, xclientSecret);
                //var authenticationResult = authenticationContext.AcquireTokenAsync(GraphService.ResourceId, xclientcred).Result;

                //var x = authenticationContext.AcquireTokenAsync(GraphService.ResourceId, xclientApplicationAppId, xCred).Result;



                authenticationResult = authenticationContext.AcquireTokenAsync(
                GraphService.ResourceId,
                _ApplicationAppId,
                _ApplicationRedirectUri,
                new PlatformParameters(PromptBehavior.RefreshSession)).Result;




                // This BookingsContainer is generated by the ODATA v4 Client Code Generator
                // See https://odata.github.io and https://github.com/odata/odata.net for usage.
                // Note that the code generator customizes the entity and property names to PascalCase
                // so they match C# guidelines, while the EDM uses lower case camelCase, as per Graph guidelies.
                // Since the application is short lived, the delegate is simply returning the authorization
                // header obtained above; a long lived application would likely need to refresh the token
                // when it expires, so it would have a slightly more complex delegate.
                /*
                var graphService = new GraphService(
                    GraphService.DefaultV1ServiceRoot,
                    () => authenticationResult.CreateAuthorizationHeader());
                */


                var graphService = new GraphService(
                    GraphService.DefaultV1ServiceRoot,
                    () =>
                    {
                        // Hvis Token timedout
                        // Forny Token
                        if (authenticationResult.ExpiresOn.ToLocalTime() >= DateTime.Now)
                        {
                            //refresh
                            // Har mulighet til å refreshe i 5 minutter etter token har utgått
                            authenticationResult = authenticationContext.AcquireTokenSilentAsync(GraphService.ResourceId, _ApplicationAppId).Result;
                            string _header = authenticationResult.CreateAuthorizationHeader();
                            return _header;
                        }
                        else
                        {
                            string _header = authenticationResult.CreateAuthorizationHeader();
                            return _header;
                        }

                    });



                // Fiddler makes it easy to look at the request/response payloads. Use it automatically if it is running.
                // https://www.telerik.com/download/fiddler
                if (System.Diagnostics.Process.GetProcessesByName("fiddler").Any())
                {
                    graphService.WebProxy = new WebProxy(new Uri("http://localhost:8888"), false);
                }



                //Sett denne til max
                graphService.MaxPageSize = 10;


                var mybusiness = graphService.BookingBusinesses.ByKey(_BookingBusinessesID);

                // http://odata.github.io/odata.net/#04-02-query-options

                BookingBusiness _business = graphService.BookingBusinesses
                    .Where(b => b.Id == _BookingBusinessesID)
                    .FirstOrDefault();

                MD5 md5 = MD5.Create();

                //var appointments = mybusiness.Appointments;
                var oDataAppointments = mybusiness.Appointments.ToArray().Where(a => System.DateTime.Parse(a.Start.DateTime) >= DateTime.Parse("21.06.2018"));

                db = new BookingEntities();
                /*
                bookingEntities.Appointment.Add(new Appointment {
                    Id = "sdgsdgsdgs"
                    
                });
                bookingEntities.Appointment.Add(new Appointment
                {
                    Id = "gfjfjfgj"

                });
                bookingEntities.SaveChanges();*/

                /*
                 
                 
                 */
                // var appointments = oDataAppointments.Where((a => bookingEntities.Appointment.Select(x => x.Id).Contains(a.Id)));

                //var temp = from x in oDataAppointments
                //           select new Appointment
                //           {
                //               customerEmailAddress = x.CustomerEmailAddress,
                //               customerId = x.CustomerId,
                //               customerName = x.CustomerName,
                //               customerPhone = x.CustomerPhone,
                //               customerNotes = x.CustomerNotes,
                //               Id = x.Id,
                //               staffMemberIds = x.StaffMemberIds.FirstOrDefault(),
                //               Start = System.DateTime.Parse(x.Start.DateTime).ToLocalTime(),
                //               End = System.DateTime.Parse(x.End.DateTime).ToLocalTime(),
                //               serviceId = x.ServiceId,
                //               serviceName = x.ServiceName,
                //               json = JsonConvert.SerializeObject(x),
                //               md5 = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(x.Id + x.Start + x.End)))
                //           };

                //foreach (var item in temp)
                foreach (var oDataAppointment in oDataAppointments)
                {
                    Appointment appointment = new Appointment
                    {
                        customerEmailAddress = oDataAppointment.CustomerEmailAddress,
                        customerId = oDataAppointment.CustomerId,
                        customerName = oDataAppointment.CustomerName,
                        customerPhone = oDataAppointment.CustomerPhone,
                        customerNotes = oDataAppointment.CustomerNotes,
                        Id = oDataAppointment.Id,
                        staffMemberIds = oDataAppointment.StaffMemberIds.FirstOrDefault(),
                        Start = System.DateTime.Parse(oDataAppointment.Start.DateTime).ToLocalTime(),
                        End = System.DateTime.Parse(oDataAppointment.End.DateTime).ToLocalTime(),
                        serviceId = oDataAppointment.ServiceId,
                        serviceName = oDataAppointment.ServiceName,
                        json = JsonConvert.SerializeObject(oDataAppointment),
                        md5 = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(oDataAppointment.Id + oDataAppointment.Start.DateTime.ToString() + oDataAppointment.End.DateTime.ToString())))
                    };

                    List<BookingAppointment> _list = new List<BookingAppointment>();
                    _list.Add(oDataAppointment);

                    switch (InsertOrUpdate(appointment))
                    {
                        case EntityState.Detached:
                            break;
                        case EntityState.Unchanged:
                            break;
                        case EntityState.Added:
                            // Ny Avtale
                            SendSMS(viaNettSMS, _business, _list, SMSTemplate.SMSConfirmation);
                            break;
                        case EntityState.Deleted:
                            break;
                        case EntityState.Modified:
                            // Avtalen er endret
                            SendSMS(viaNettSMS, _business, _list, SMSTemplate.SMSUpdate);
                            break;
                        default:
                            break;
                    }
                }

                List<string> odataIDs = oDataAppointments.Select(x => x.Id).ToList<string>();

                var deleteAppointments = from a in db.Appointment
                                         where !odataIDs.Contains(a.Id)
                                         select a;

                var bappointments = new List<BookingAppointment>();
                
                foreach (var appoint in deleteAppointments)
                {
                    var bappoint = JsonConvert.DeserializeObject<BookingAppointment>(appoint.json);
                    bappointments.Add(bappoint);
                }

                SendSMS(viaNettSMS, _business, bappointments, SMSTemplate.SMSCancellation);

                var delList = deleteAppointments.ToList();

                db.Appointment.RemoveRange(deleteAppointments);
                db.SaveChanges();

                Console.ReadLine();
                /*
                                foreach (var appointment in oDataAppointments)
                                {
                                    hash = md5.ComputeHash(Encoding.UTF8.GetBytes(appointment.Id + appointment.Start + appointment.End));

                                    bookingEntities.Appointment.Add(new Appointment
                                    {
                                        customerEmailAddress = appointment.CustomerEmailAddress,
                                        customerId = appointment.CustomerId,
                                        customerName = appointment.CustomerName,
                                        customerPhone = appointment.CustomerPhone,
                                        customerNotes = appointment.CustomerNotes,
                                        Id = appointment.Id,
                                        staffMemberIds = appointment.StaffMemberIds.FirstOrDefault(),
                                        Start = System.DateTime.Parse(appointment.Start.DateTime),
                                        End = System.DateTime.Parse(appointment.End.DateTime),
                                        serviceId = appointment.ServiceId,
                                        serviceName = appointment.ServiceName,
                                        json = appointment.ToString(),
                                        md5 = hash.ToString()
                                    });
                                    bookingEntities.SaveChanges();

                                }
                */

                md5.Dispose();

                while (true)
                {
                    //Console.WriteLine(graphService.BookingBusinesses.FirstOrDefault().Id);
                    ///System.Threading.Thread.Sleep(3000);
                    //Console.WriteLine(authenticationResult.ExpiresOn.ToLocalTime().ToString());    
                    System.Threading.Thread.Sleep(3000);

                }

                //var mybusiness = graphService.BookingBusinesses.ByKey("Karrieresenter1@ostfoldfk.no").Appointments.Where(a => (Microsoft.OData.Edm.Date.Parse(a.Start.DateTime)) < Microsoft.OData.Edm.Date.Now.AddDays(-2));

                BookingBusiness businessKarsen = graphService.BookingBusinesses.Where(b => b.Id == "Karrieresenter1@ostfoldfk.no").FirstOrDefault();
                var mybookingAppointments = graphService.BookingBusinesses.ByKey("Karrieresenter1@ostfoldfk.no").Appointments.GetAllPages().ToArray();

                foreach (BookingAppointment item in mybookingAppointments)
                {
                    Console.WriteLine("{0} have appointment: {1}", item.CustomerName, item.Start.DateTime);
                }

                var appointments2 = graphService.BookingBusinesses.FirstOrDefault().Appointments.Where(a => a.Price == 0);

                // Get the list of booking businesses that the logged on user can see.
                // NOTE: I'm not using 'async' in this sample for simplicity;
                // the ODATA client library has full support for async invocations.
                var bookingBusinesses = graphService.BookingBusinesses.ToArray();



                // var bookingBusinesses = graphService.BookingBusinesses.Expand("Appointments").ToArray();






                foreach (var _ in bookingBusinesses)
                {
                    Console.WriteLine(_.DisplayName);
                    foreach (var appointment in _.Appointments)
                    {
                        Console.WriteLine(appointment.CustomerName);
                    }
                }


                if (bookingBusinesses.Length == 0)
                {
                    Console.WriteLine("Enter a name for a new booking business, or leave empty to exit.");
                }
                else
                {
                    Console.WriteLine("Type the name of the booking business to use or enter a new name to create a new booking business, or leave empty to exit.");
                }

                var businessName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(businessName))
                {
                    return;
                }

                // See if the name matches one of the entities we have (this is searching the local array)
                var bookingBusiness = bookingBusinesses.FirstOrDefault(_ => _.DisplayName == businessName);
                if (bookingBusiness == null)
                {
                    // If we don't have a match, create a new bookingBusiness.
                    // All we need to pass is the display name, but we could pass other properties if needed.
                    // This NewEntityWithChangeTracking is a custom extension to the standard ODATA library to make it easy.
                    // Keep in mind there are other patterns that could be used, revolving around DataServiceCollection.
                    // The trick is that the data object must be tracked by a DataServiceCollection and then we need
                    // to save with SaveChangesOptions.PostOnlySetProperties.
                    bookingBusiness = graphService.BookingBusinesses.NewEntityWithChangeTracking();
                    bookingBusiness.DisplayName = businessName;
                    Console.WriteLine("Creating new booking business...");
                    graphService.SaveChanges(SaveChangesOptions.PostOnlySetProperties);

                    Console.WriteLine($"Booking Business Created: {bookingBusiness.Id}. Press any key to continue.");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Using existing booking business.");
                }

                // Play with the newly minted booking business
                var business = graphService.BookingBusinesses.ByKey(bookingBusiness.Id);

                // Add an external staff member (these are easy, as we don't need to find another user in the AD).
                // For an internal staff member, the application might query the user or the Graph to find other users.
                var staff = business.StaffMembers.FirstOrDefault();
                if (staff == null)
                {
                    staff = business.StaffMembers.NewEntityWithChangeTracking();
                    staff.EmailAddress = "staff1@contoso.com";
                    staff.DisplayName = "Staff1";
                    staff.Role = BookingStaffRole.ExternalGuest;
                    Console.WriteLine("Creating staff member...");
                    graphService.SaveChanges(SaveChangesOptions.PostOnlySetProperties);
                    Console.WriteLine("Staff created.");
                }
                else
                {
                    Console.WriteLine($"Using staff member {staff.DisplayName}");
                }

                // Add an Appointment
                var newAppointment = business.Appointments.NewEntityWithChangeTracking();
                newAppointment.CustomerEmailAddress = "customer@contoso.com";
                newAppointment.CustomerName = "John Doe";
                newAppointment.ServiceId = business.Services.First().Id; // assuming we didn't deleted all services; we might want to double check first like we did with staff.
                newAppointment.StaffMemberIds.Add(staff.Id);
                newAppointment.Reminders.Add(new BookingReminder { Message = "Hello", Offset = TimeSpan.FromHours(1), Recipients = BookingReminderRecipients.AllAttendees });
                var start = DateTime.Today.AddDays(1).AddHours(13).ToUniversalTime();
                var end = start.AddHours(1);
                newAppointment.Start = new DateTimeTimeZone { DateTime = start.ToString("o"), TimeZone = "UTC" };
                newAppointment.End = new DateTimeTimeZone { DateTime = end.ToString("o"), TimeZone = "UTC" };
                Console.WriteLine("Creating appointment...");
                graphService.SaveChanges(SaveChangesOptions.PostOnlySetProperties);
                Console.WriteLine("Appointment created.");

                // Query appointments.
                // Note: the server imposes a limit on the number of appointments returned in each request
                // so clients must use paging or request a calendar view with business.GetCalendarView().
                foreach (var appointment in business.Appointments.GetAllPages())
                {
                    // DateTimeTimeZone comes from Graph and it uses string for the DateTime, not sure why.
                    // Perhaps we could tweak the generated proxy (or add extension method) to automatically 
                    // do this ToString/Parse for us, so it does not pollute the entire code.
                    Console.WriteLine($"{DateTime.Parse(appointment.Start.DateTime).ToLocalTime()}: {appointment.ServiceName} with {appointment.CustomerName}");
                }

                // In order for customers to interact with the booking business we need to publish its public page.
                // We can also Unpublish() to hide it from customers, but where is the fun in that?
                Console.WriteLine("Publishing booking business public page...");
                business.Publish().Execute();

                // Let the user play with the public page
                Console.WriteLine(business.GetValue().PublicUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }

        private static void SendSMS(ViaNettSMS viaNettSMS, BookingBusiness _business, IEnumerable<BookingAppointment> _appointments, SMSTemplate sMSTemplate)
        {
            foreach (BookingAppointment appointment in _appointments)
            {
                // Send SMS to Customer
                Console.WriteLine("Sending SMS to CustomerName: {0}, CustomerPhone: {1}", appointment.CustomerName, appointment.CustomerPhone);

                ViaNettSMS.Result result;
                string _appointmentDateString = DateTime.Parse(appointment.Start.DateTime).ToString("dd.MM.yyyy HH:mm");

                string message = RenderSMSTemplate(_business, appointment, _appointmentDateString, sMSTemplate);


                try
                {
                    // Send SMS through HTTP API
                    Console.WriteLine("SendingSMS: {0} {1} {2}", _SMSSenderFrom, appointment.CustomerPhone, message);
                    result = viaNettSMS.SendSMS(_SMSSenderFrom, appointment.CustomerPhone, message);
                    //result = viaNettSMS.SendSMS(_SMSSenderFrom, "40453626", message);

                    // Show Send SMS response
                    if (result.Success)
                    {
                        Debug.WriteLine("Message successfully sent");
                    }
                    else
                    {
                        Debug.WriteLine("Received error: " + result.ErrorCode + " " + result.ErrorMessage);
                    }
                }
                catch (System.Net.WebException ex)
                {
                    //Catch error occurred while connecting to server.
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private static string RenderSMSTemplate(BookingBusiness _business, BookingAppointment appointment, string _appointmentDateString, SMSTemplate sMSTemplate)
        {
            string smsTemplateBody = ConfigurationManager.AppSettings[sMSTemplate.ToString()]; // String.Format(, appointment.ServiceName, _appointmentDateString);

            string messageFooter = ConfigurationManager.AppSettings["SMSFooter"];
            string message = (smsTemplateBody + messageFooter);

            message = message
                                .Replace("%BookingAppointment.CustomerName%", appointment.CustomerName)
                                .Replace("%BookingAppointment.CustomerEmailAddress%", appointment.CustomerEmailAddress)
                                .Replace("%BookingAppointment.CustomerPhone%", appointment.CustomerPhone)
                                .Replace("%BookingAppointment.ServiceName%", appointment.ServiceName)
                                .Replace("%BookingAppointment.Start%", System.DateTime.Parse(appointment.Start.DateTime).ToLocalTime().ToString())
                                .Replace("%BookingAppointment.End%", System.DateTime.Parse(appointment.End.DateTime).ToLocalTime().ToString())
                                .Replace("%BookingAppointment.Duration%", appointment.Duration.Minutes.ToString())
                                .Replace("%BookingAppointment.ServiceLocation.DisplayName%", appointment.ServiceLocation.DisplayName)
                                .Replace("%BookingAppointment.ServiceLocation.LocationEmailAddress%", appointment.ServiceLocation.LocationEmailAddress)
                                .Replace("%BookingAppointment.ServiceLocation.Address.Street%", appointment.ServiceLocation.Address.Street)
                                .Replace("%BookingAppointment.ServiceLocation.Address.City%", appointment.ServiceLocation.Address.City)
                                .Replace("%BookingAppointment.ServiceLocation.Address.State%", appointment.ServiceLocation.Address.State)
                                .Replace("%BookingAppointment.ServiceLocation.Address.CountryOrRegion%", appointment.ServiceLocation.Address.CountryOrRegion)
                                .Replace("%BookingAppointment.ServiceLocation.Address.PostalCode%", appointment.ServiceLocation.Address.PostalCode)
                                .Replace("%BookingAppointment.ServiceLocation.Coordinates.Altitude%", appointment.ServiceLocation.Coordinates.Altitude.ToString())
                                .Replace("%BookingAppointment.ServiceLocation.Coordinates.Latitude%", appointment.ServiceLocation.Coordinates.Latitude.ToString())
                                .Replace("%BookingAppointment.ServiceLocation.Coordinates.Longitude%", appointment.ServiceLocation.Coordinates.Longitude.ToString())
                                .Replace("%BookingAppointment.ServiceLocation.Coordinates.Accuracy%", appointment.ServiceLocation.Coordinates.Accuracy.ToString())
                                .Replace("%BookingAppointment.ServiceLocation.Coordinates.AltitudeAccuracy%", appointment.ServiceLocation.Coordinates.AltitudeAccuracy.ToString())
                                .Replace("%BookingBusiness.Address.City%", _business.Address.City)
                                .Replace("%BookingBusiness.Address.Street%", _business.Address.Street)
                                .Replace("%BookingBusiness.Address.PostalCode%", _business.Address.PostalCode)
                                .Replace("%BookingBusiness.Address.State%", _business.Address.State)
                                .Replace("%BookingBusiness.Address.CountryOrRegion%", _business.Address.CountryOrRegion)
                                .Replace("%BookingBusiness.DisplayName%", _business.DisplayName)
                                .Replace("%BookingBusiness.Email%", _business.Email)
                                .Replace("%BookingBusiness.Phone%", _business.Phone)
                                .Replace("%BookingBusiness.PublicUrl%", _business.PublicUrl)
                                .Replace("%BookingBusiness.WebSiteUrl%", _business.WebSiteUrl);

           
            message = message.Replace("\\n", Environment.NewLine);
            return message;
        }
        public static EntityState InsertOrUpdate(Appointment appointment)
        {

            if (db.Appointment.Any(a => a.Id == appointment.Id) &&
                db.Appointment.Any(a => a.md5 == appointment.md5))
            {
                db.Appointment.Attach(appointment);
                db.Entry(appointment).State = EntityState.Unchanged;
                db.SaveChanges();
                return EntityState.Unchanged;
            }
            else
            if (db.Appointment.Any(a => a.Id == appointment.Id) &&
                db.Appointment.Any(a => a.md5 != appointment.md5))
            {
                db.Appointment.Attach(appointment);
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return EntityState.Modified;
            }
            else
            {
                db.Appointment.Attach(appointment);
                db.Entry(appointment).State = EntityState.Added;
                db.Appointment.Add(appointment);
                db.SaveChanges();
                return EntityState.Added;
            }
        }

    }
}
