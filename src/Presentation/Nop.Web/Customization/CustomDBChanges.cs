using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing;
using FluentMigrator;
using Nop.Core.Domain.Localization;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using Nop.Services.Localization;

namespace Nop.Web.Customization
{
    public class CustomDBChanges
    {
        public static void ApplyLocalResources(ILocalizationService localizationService)
        {
            //Adding English locales
            localizationService.AddOrUpdateLocaleResourceAsync(languageId: 1, resources: new Dictionary<string, string>
            {
                ["admin.configuration.stores.info"] = "Info",

                ["Hoho.Home.ViewProducts.Title"] = "Best sellers",
                ["Hoho.Home.ViewProducts.Description"] = "Discover a list of popular experience gifts",
                ["Hoho.Home.ViewProducts.AllBtn"] = "View all",
                ["Hoho.Home.ViewProducts.CarItemCTA"] = "Explore more exciting services",
                ["Hoho.Home.ViewProducts.CarItemLink"] = "Go to catalog >>",
                ["Hoho.Home.ViewProducts.ViewAllCTA_Title"] = "Discover unique gift experiences",
                ["Hoho.Home.ViewProducts.ViewAllCTA_Decr"] = "Explore a variety of choice of the best gift ever. Create brilliant memories for yourself or your loved ones",
                ["Hoho.Home.ViewProducts.ViewAllCTA_Shop"] = "Let's start",
                ["Hoho.Home.ViewProducts.AlViewAllCTA_learn"] = "Learn more",

                ["Hoho.General.ReadMore"] = "Read more",

                ["HohoCustomization.Home.ChooseWhoGets.Title"] = "Choose who the gift is for",
                ["HohoCustomization.Home.ChooseWhoGets.Description"] = "It will help you to choose something, that will surely please and delight the recipient.</br>Personalization is the key to genuine joy!",

                ["HohoCustomization.Home.DecideWhatGift.Title"] = "Decide what to gift",
                ["HohoCustomization.Home.DecideWhatGift.Description"] = "Choose up to 10 services that would fit the recipient the most.</br>You have a chance to fulfill a dream or turn your gift into a thoughtful expression of care and love.",

                ["HohoCustomization.Home.SelectHowGift.Title"] = "Select how to present the gift",
                ["HohoCustomization.Home.SelectHowGift.Description"] = "Just choose a method that is the most suitable for your occasion.",

                ["Products.Manufacturer"] = "Provider",
                ["Products.Manufacturers"] = "Providers",

                ["Products.Sku"] = "id",

                ["Products.Photos"] = "Photos",
                ["Products.Description"] = "Details",
                ["Products.Locations"] = "Location",
                ["Products.Reviews"] = "Reviews",
                ["Products.HowDoesItWork"] = "How does it work",
                ["Products.faq"] = "Questions and answers",
                ["Product.ReviewsBlock.ShowMore"] = "Show more",
                ["Product.ReviewsBlock.Hide"] = "Hide",

                ["Catalog.ForWho"] = "For who",
                ["Catalog.ShowFilterBtn"] = "Filter",
                ["Catalog.Filter.Show"] = "Show",
                ["Catalog.Filter.Close"] = "Close",

                ["Header.Top.ActivateBtn"] = "Activate certificate",
                ["HeaderPanel.InfoModule.Btn"] = "Information",
                ["HeaderPanel.Menu.Btn"] = "Menu",
                ["HeaderPanel.GiftCatalog.Btn"] = "All gifts",


                //Activate pages
                ["PageTitle.Redeem.Check"] = "Check certificate",
                ["PageTitle.Redeem"] = "Activate certificate",
                ["PageTitle.Redeem.Activated"] = "Certificate activated",

                ["Redeem.Check.PageTitle"] = "Activate certificate",
                ["Redeem.Check.Step01"] = "Check",
                ["Redeem.Check.Step02"] = "Choose",
                ["Redeem.Check.Step03"] = "Activate",
                ["Redeem.Check.CallToAction"] = "Enter a unique number of your certificate to see what is included into the gift.",

                ["Redeem.Activate.Title"] = "Congratulations!",
                ["Redeem.Activate.Subtitle"] = "Your certificate is the key to unforgettable experiences.",
                ["Redeem.Activate.FolowInstruction"] = "Follow the instruction, paint your life.",

                ["Redeem.Fields.Valid.Required"] = "This field is required",
                ["Redeem.Fields.Valid.CNFormat"] = "The value of the field should match the format: XX-XX-XX-XX. Where X is a natural number",
                ["Redeem.Fields.Valid.SecurityFormat"] = "The value of the field should match the format: XX-XX. Where X is a natural number",
                ["Redeem.IncorrectSecurityCode"] = "Incorrect secret code. Please pay attention that after 3 mistakes, the card will be blocked",
                ["Redeem.Check.Button"] = "Check certificate",

                ["Redeem.Check.CardNotFound"] = "Oops, something went wrong...</br>We can't find such certificate. Carefully check the number and try again.",
                ["Redeem.Check.CardExpired"] = "We are sorry, but the certificate has expired on {0}.",
                ["Redeem.Check.CardRedeemed"] = "This certificate was already used on {0}",
                ["Redeem.Check.CardBlocked"] = "We are sorry, but the certificate was blocked on {0}. You can inquire about the reason for the block by contacting our customer service center.",

                ["Redeem.Coose.Title"] = "Important!!!",
                ["Redeem.Choose.ActivateTill"] = "Activate certificate before <span class=\"highlight\">{0}</span> (dd/mm/yyyy).",
                ["Redeem.Choose.Explore"] = "Explore the services included in your gift.",
                ["Redeem.Choose.Choose"] = "Among the offered services, <span class=\"highlight\">choose the one</span> you like the most.",
                ["Redeem.Coose.ChooseBtn"] = "Choose to activate",

                ["Redeem.Activate.FillOut"] = "Fill in the activation form.",
                ["Redeem.Activate.FillDate"] = "Specify your desired date and time for getting the experience.",
                ["Redeem.Activate.ManagerCall"] = "Our manager will contact you to coordinate the exact date and time.",
                ["Redeem.Activate.WeBook"] = "We will reserve the experience and send an SMS with the booking date and time. All other necessary information will be sent to your email.",
                ["Redeem.Fields.CardNumber"] = "Card number",
                ["Redeem.Fields.SecurityCode"] = "Security code",
                ["Redeem.Fields.Comment"] = "Specify your desired date and time of visit",
                ["Redem.ActivateButton"] = "Activate certificate",

                ["Redeem.IRedDescription.PleaseAccept"] = "Please carefully read the service description and confirm it by marking the checkbox.",
                ["Redeem.IRedDescription.IAccept"] = "I fully read description of the service which I activate.",

                ["Redeem.Congratulations.Wishes"] = "We wish you to spend time enjoying every moment.",
                ["Redeem.Congratulations.LeaveFeadback"] = "If you like the chosen service and our platform, we would appreciate to see your feedback.",
                ["Redeem.Congratulations.Okay"] = "Ok",
                ["Redeem.Congratulations.PageTitle"] = "Congratulations!",
                ["Redeem.Congratulations.Success"] = "Your certificate has been successfully activated. We will contact you shortly to confirm the booking details.",

                ["Redeem.Activated.PageTitle"] = "Card activation status",
                ["Redeem.Activated.RedeemTill"] = "Use the activated service before <span class=\"highlight\">{0}</span> (dd/mm/yyyy).",
                ["Redeem.Activated.Service"] = "The activated service is:",
                ["Redeem.Activated.Status"] = "Status",
                ["Redeem.Activated.StatusValid"] = "Valid (Activated)",
                ["Redeem.Activated.StatusRedeemed"] = "Invalid (That certificate is already used)",

                ["CheckoutPanel.Title"] = "Hooray. Just few steps and the best gift is in your hands...",
                ["CheckoutPanel.CI.Title"] = "Customer info",
                ["CheckoutPanel.FH.Title"] = "Who are you buying for?",
                ["CheckoutPanel.SP.Title"] = "Select packaging type",
                ["CheckoutPanel.RI.Title"] = "Recipient info",
                ["CheckoutPanel.DI.Title"] = "Delivery options",
                ["CheckoutPanel.FH.AsGift"] = "I'm buying as a gift",
                ["CheckoutPanel.FH.ForMyself"] = "I'm buying for myself",
                ["CheckoutPanel.SP.GiftPackaging"] = "Gift packaging",
                ["CheckoutPanel.SP.GiftPackagingDescription"] = "you can pickup it in our store or we will deliver it to an address that you wish",
                ["CheckoutPanel.SP.Certificate"] = "Electronic certificate",
                ["CheckoutPanel.SP.CertificateDescription"] = "if time is limited or you want to buy a service for yourself",
                ["CheckoutPanel.SP.CertificateDescription2"] = "*free delivery in just few minutes to any Email",
                ["CheckoutPanel.DI.Pickup"] = "Pickup in store",
                ["CheckoutPanel.DI.SendToMe"] = "Deliver to me.",
                ["CheckoutPanel.DI.SendToRecip"] = "Deliver to other recipient",
                ["CheckoutPanel.Warning.NoBoxes"] = "We are sorry, but all packaging products are temporary out of stock",
                ["CheckoutPanel.CustomerInfo.Warning"] = "Please fill-in all required fields",
                ["CheckoutPanel.Tabs.Packaging"] = "Packaging",
                ["CheckoutPanel.Tabs.Contacts"] = "Contacts",
                ["CheckoutPanel.Tabs.Delivery"] = "Delivery",
                ["CheckoutPanel.Tabs.Payment"] = "Payment",
                ["CheckoutPanel.CS.BackBtn"] = "Back",
                ["CheckoutPanel.CS.NextBtn"] = "Next",
                ["CheckoutPanel.CS.PayBtn"] = "Pay",

                ["Hoho.HowToGift.GiftBox.Title"] = "Gift Box",
                ["Hoho.HowToGift.GiftBox.Details"] = "to create a sense of a true celebration",
                ["Hoho.HowToGift.Envelope.Title"] = "Envelope",
                ["Hoho.HowToGift.Envelope.Details"] = "if you want to make the gift compact",
                ["Hoho.HowToGift.ECert.Title"] = "Electronic certificate",
                ["Hoho.HowToGift.ECert.Details"] = "if time is limited or you want to buy a service for yourself",

                ["Product.Reviews.NoReviewsYet"] = "No reviews at the moment",
                ["Product.Reviews.WhoCanLeave"] = "Only users who have already tried the service can leave reviews.",
                ["Product.Reviews.AddReview"] = "Add review",
                ["Product.Reviews.CTA_AddReview"] = "Add your reviews and help people make the best choice.",
                ["Product.Reviews.ReedAll"] = "Reed all reviews",
                ["Product.Vendor.Contacts"] = "Contacts",
                ["Product.Vendor.GetDir"] = "Get directions...",
                ["Product.Vendor.Address"] = "Address",

                ["enums.nop.core.domain.catalog.producttype.multicertificate"] = "Multicertificate",
                ["enums.nop.core.domain.catalog.producttype.PackagingOfMulticertificate"] = "Packaging for multicertificate",
                ["enums.nop.core.domain.catalog.producttype.ApplyToMulticertificate"] = "Apply to Multicertificate",
                ["admin.product.producttype.none"] = "None",

                ["EUCookieLaw.OK"] = "Understood",
            });


            //Adding Georgian locales
            localizationService.AddOrUpdateLocaleResourceAsync(languageId: 2, resources: new Dictionary<string, string>
            {
                ["admin.configuration.stores.info"] = "ინფორმაცია",

                ["Hoho.Home.ViewProducts.Title"] = "ბესტსელერები",
                ["Hoho.Home.ViewProducts.Description"] = "აღმოაჩინეთ პოპულარული გამოცდილებების საჩუქრების სია",
                ["Hoho.Home.ViewProducts.AllBtn"] = "ყველას ნახვა",
                ["Hoho.Home.ViewProducts.CarItemCTA"] = "აღმოაჩინეთ უფრო მეტი საინტერესო სერვისები",
                ["Hoho.Home.ViewProducts.CarItemLink"] = "გადადით კატალოგში >>",
                ["Hoho.Home.ViewProducts.ViewAllCTA_Title"] = "აღმოაჩინეთ უნიკალური საჩუქრების გამოცდილება",
                ["Hoho.Home.ViewProducts.ViewAllCTA_Decr"] = "გამოიკვლიეთ საუკეთესო საჩუქრის მრავალფეროვანი არჩევანი. შექმენით ბრწყინვალე მოგონებები თქვენთვის ან თქვენი საყვარელი ადამიანებისთვის",
                ["Hoho.Home.ViewProducts.ViewAllCTA_Shop"] = "იშოპინგე",
                ["Hoho.Home.ViewProducts.AlViewAllCTA_learn"] = "გაიგე მეტი",

                ["Hoho.General.ReadMore"] = "წაიკითხე მეტი",

                ["HohoCustomization.Home.ChooseWhoGets.Title"] = "აირჩიეთ ვისთვის არჩევ საჩუქარს",
                ["HohoCustomization.Home.ChooseWhoGets.Description"] = "იგი დაგეხმარებათ არჩევანში,რათა გაახაროთ საჩუქრის მიმღები </br>პერსონალიზაცია ბედნიერების წყაროა!",

                ["HohoCustomization.Home.DecideWhatGift.Title"] = "გადაწყვიტეთ რა გსურთ აჩუქოთ",
                ["HohoCustomization.Home.DecideWhatGift.Description"] = "შეარჩიეთ 1-დან 10-მდე სერვისი, რომელიც თქვენი აზრით მოეწონება საჩუქრის მიმღებს.</br>თქვენ გექნებათ საშუალება აუხდინოთ ოცნება და აჩუქოთ ახალი ემოციები",

                ["HohoCustomization.Home.SelectHowGift.Title"] = "აირჩიეთ,თუ როგორ გსურთ საჩუქრის მირთმევა",
                ["HohoCustomization.Home.SelectHowGift.Description"] = "უბრალოდ აირჩიეთ  მეთოდი, რომელიც ყველაზე შესაფერისია თქვენი სიტუაციისთვის.",
                ["Products.Manufacturer"] = "მიმწოდებელი",
                ["Products.Manufacturers"] = "მიმწოდებლები",

                ["Products.Sku"] = "id",

                ["Products.Photos"] = "სურათები",
                ["Products.Description"] = "აღწერა",
                ["Products.Locations"] = "ადგილი",
                ["Products.Reviews"] = "შეფასება",
                ["Products.HowDoesItWork"] = "როგორ მუშაობს",
                ["Products.faq"] = "კითხვები და პასუხები",
                ["Product.ReviewsBlock.ShowMore"] = "მეტის ნახვა",
                ["Product.ReviewsBlock.Hide"] = "დამალვა",

                ["Catalog.ForWho"] = "ვისთვის",
                ["Catalog.ShowFilterBtn"] = "ფილტრი",
                ["Catalog.Filter.Show"] = "ნახვა",
                ["Catalog.Filter.Close"] = "დახურვა",

                ["Header.Top.ActivateBtn"] = "სერტიფიკატის გააქტიურება",
                ["HeaderPanel.InfoModule.Btn"] = "ინფორმაცია",
                ["HeaderPanel.Menu.Btn"] = "მენიუ",
                ["HeaderPanel.GiftCatalog.Btn"] = "ყველა საჩუქარი",


                //Activate pages
                ["PageTitle.Redeem.Check"] = "სერტიფიკატის გადამოწმება",
                ["PageTitle.Redeem"] = "სერტიფიკატის გააქტიურება",
                ["PageTitle.Redeem.Activated"] = "სერტიფიკატი გააქტიურებულია",


                ["Redeem.Check.PageTitle"] = "სერტიფიკატის გააქტიურება",
                ["Redeem.Check.Step01"] = "შემოწმება",
                ["Redeem.Check.Step02"] = "არჩევა",
                ["Redeem.Check.Step03"] = "გააქტიურება",
                ["Redeem.Check.CallToAction"] = "შეიყვანეთ თქვენი სერტიფიკატის ნომერი,რათა გაეცნოთ თუ რას მოიცავს თქვენი საჩუქარი.",

                ["Redeem.Activate.Title"] = "გილოცავთ!",
                ["Redeem.Activate.Subtitle"] = "თქვენი სერტიფიკატი დაუვიწყარი შთაბეჭდილებებისა და ემოციების გასაღებია",
                ["Redeem.Activate.FolowInstruction"] = "მიჰყევით ინსტრუქციას და გაიფერადეთ ცხოვრება.",

                ["Redeem.Fields.Valid.Required"] = "გთხოვთ შეავსოთ მითითებული ველი",
                ["Redeem.Fields.Valid.CNFormat"] = "ველი უნდა შეიცავდეს კონკრეტულ ფორმატს : XX-XX-XX-XX. X სიმბოლო-ნატურალური რიცხვია",
                ["Redeem.Fields.Valid.SecurityFormat"] = "ველი უნდა შეიცავდეს კონკრეტულ ფორმატს : XX-XX. X სიმბოლო-ნატურალური რიცხვია",
                ["Redeem.IncorrectSecurityCode"] = "კოდი არასწორია.გთხოვთ გაითვალისწინოთ,რომ 3-ჯერ კოდის არასწორად ჩაწერისას თქვენი ბარათი იქნება დაბლოკილი",
                ["Redeem.Check.Button"] = "შეამოწმეთ სერტიფიკატი",

                ["Redeem.Check.CardNotFound"] = "უკაცრავად რაღაცა არასწორია.</br>ასეთი სერტიფიკატი არ მოიძებნა. გთხოვთ გადაამოწმოთ თქვენი სერტიფიკატი და სცადოთ თავიდან.",
                ["Redeem.Check.CardExpired"] = "სამწუხაროდ, არსებულ სერთიფიკატს ვადა ამოეწურა {0}",
                ["Redeem.Check.CardRedeemed"] = "სერტიფიკატი გამოყენებულია {0}",
                ["Redeem.Check.CardBlocked"] = "ვწუხვართ,სერთუფიკატი იქნა დაბლოკილი {0}.პრობლემის აღმოსაფრქველად გთხოვთ ეწვიოთ საიტზე არსებულ დახმარების ჯგუფს",


                ["Redeem.Coose.Title"] = "ყურადღება!!!",
                ["Redeem.Choose.ActivateTill"] = "სერტიფიკატი ხელმისაწვდომია</br><span class=\"highlight\">{0}</span> (დდ/თთ/წწწწ).",
                ["Redeem.Choose.Explore"] = "გხოვთ გაეცანოთ სერვისებს,რომლებსაც მოიცავს თქვენი საჩუქარი",
                ["Redeem.Choose.Choose"] = "არსებული სერვისებიდან, <span class=\"highlight\">აირჩიეთ ერთ-ერთი</span> რომელიც უფრო მოგწონთ.",
                ["Redeem.Coose.ChooseBtn"] = "გაააქტიურეთ",

                ["Redeem.Activate.FillOut"] = "შეავსეთ აქტივაციის ფორმა.",
                ["Redeem.Activate.FillDate"] = "მიუთითეთ თქვენთვის სასურველი დრო და თარიღი სერვისის მისაღებად",
                ["Redeem.Activate.ManagerCall"] = "ჩვენი მენეჯერი დაგიკავშირდებათ, შეგითანხმებთ დროსა და თარიღს",
                ["Redeem.Activate.WeBook"] = "ჩვენ დაგიჯავშნით სერვისს და დაჯავშნის თარიღსა და დროს გამოგიგზავნით SMS-ის სახით. ყველა სხვა საჭირო ინფორმაცია გაიგზავნება თქვენს ელ.ფოსტაზე.",
                ["Redeem.Fields.CardNumber"] = "ბარათის ნომერი",
                ["Redeem.Fields.SecurityCode"] = "დაცვის კოდი",
                ["Redeem.Fields.Comment"] = "მიუთითეთ ვიზიტის სასურველი თარიღი და დრო",
                ["Redem.ActivateButton"] = "სერტიფიკატის გააქტიურება",


                ["Redeem.IRedDescription.PleaseAccept"] = "გთხოვთ, ყურადღებით წაიკითხოთ სერვისის აღწერა და დაადასტუროთ ის მონიშვნით.",
                ["Redeem.IRedDescription.IAccept"] = "სრულად წავიკითხე იმ სერვისის აღწერა, რომელსაც ვააქტიურებ.",

                ["Redeem.Congratulations.Wishes"] = "გისურვებთ სიამოვნება მიიღოთ ყოველი მომენტით.",
                ["Redeem.Congratulations.LeaveFeadback"] = "თუ მოგწონთ არჩეული სერვისი და ჩვენი პლატფორმა, მადლობელნი ვიქნებით თქვენი უკუკავშირით.",
                ["Redeem.Congratulations.Okay"] = "კარგი",
                ["Redeem.Congratulations.PageTitle"] = "გილოცავთ!",
                ["Redeem.Congratulations.Success"] = "თქვენი სერტიფიკატი წარმატებით გააქტიურდა. ჩვენ მალე დაგიკავშირდებით, თქვენი ჯავშნის მონაცემების დასადასტურებლად.",

                ["Redeem.Activated.PageTitle"] = "ბარათის აქტივაციის სტატუსი",
                ["Redeem.Activated.RedeemTill"] = "გამოიყენეთ გააქტიურებული სერვისი <span class=\"highlight\">{0}</span> წლამდე (dd/mm/yyyy).",
                ["Redeem.Activated.Service"] = "გააქტიურებული სერვისია:",
                ["Redeem.Activated.Status"] = "სტატუსი",
                ["Redeem.Activated.StatusValid"] = "მოქმედებს (გააქტიურებულია)",
                ["Redeem.Activated.StatusRedeemed"] = "არასწორი (ეს სერტიფიკატი უკვე გამოყენებულია)",

                ["CheckoutPanel.Title"] = "რამდენიმე ნაბიჯიც და საუკეთესო საჩუქარი თქვენს ხელში იქნება...",
                ["CheckoutPanel.CI.Title"] = "მომხმარებლის ინფორმაცია",
                ["CheckoutPanel.FH.Title"] = "ვისთვის ყიდულობთ?",
                ["CheckoutPanel.SP.Title"] = "შეარჩიეთ შეფუთვა",
                ["CheckoutPanel.RI.Title"] = "მიმღების ინფორმაცია",
                ["CheckoutPanel.DI.Title"] = "მიწოდების ვარიანტები",
                ["CheckoutPanel.FH.AsGift"] = "ვყიდულობ საჩუქრად",
                ["CheckoutPanel.FH.ForMyself"] = "ვყიდულობ ჩემთვის",
                ["CheckoutPanel.SP.GiftPackaging"] = "სასაჩუქრე შეფუთვა",
                ["CheckoutPanel.SP.GiftPackagingDescription"] = "შეგიძლიათ თვითონ წაიღოთ ან ჩვენ მოგაწოდებთ თქვენთვის სასურველ მისამართზე",
                ["CheckoutPanel.SP.Certificate"] = "ელექტრონული სერტიფიკატი",
                ["CheckoutPanel.SP.CertificateDescription"] = "თუ გეჩქარებათ, გსურთ აჩუქოთ დისტანციურად ან გსურთ შეიძინოთ თქვენთვის",
                ["CheckoutPanel.SP.CertificateDescription2"] = "*უფასო მიწოდება რამდენიმე წუთში ნებისმიერ იმეილზე",
                ["CheckoutPanel.DI.Pickup"] = "მაღაზიიდან გავიტან",
                ["CheckoutPanel.DI.SendToMe"] = "მომიტანეთ",
                ["CheckoutPanel.DI.SendToRecip"] = "მიაწოდეთ საჩუქრის მიმღებს",
                ["CheckoutPanel.Warning.NoBoxes"] = "ვწუხვართ, შესაფუთი პროდუქტების მარაგი დროებით ამოწურულია",
                ["CheckoutPanel.CustomerInfo.Warning"] = "გთხოვთ შეავსოთ ყველა სავალდებულო ველი",
                ["CheckoutPanel.Tabs.Packaging"] = "შეფუთვა",
                ["CheckoutPanel.Tabs.Contacts"] = "კონტაქტები",
                ["CheckoutPanel.Tabs.Delivery"] = "მიწოდება",
                ["CheckoutPanel.Tabs.Payment"] = "გადახდა",
                ["CheckoutPanel.CS.BackBtn"] = "უკან",
                ["CheckoutPanel.CS.NextBtn"] = "შემდეგი",
                ["CheckoutPanel.CS.PayBtn"] = "გადახდა",


                ["Hoho.HowToGift.GiftBox.Title"] = "სასაჩუქრე ყუთი",
                ["Hoho.HowToGift.GiftBox.Details"] = "შექმენით დღესასწაულის განწყობა",
                ["Hoho.HowToGift.Envelope.Title"] = "კონვერტი",
                ["Hoho.HowToGift.Envelope.Details"] = "თუ გსურთ რომ საჩუქარი იყოს კომპაქტური ",
                ["Hoho.HowToGift.ECert.Title"] = "ელექტრონული სერტიფიკატი",
                ["Hoho.HowToGift.ECert.Details"] = "თუ გეჩქარებათ, გსურთ აჩუქოთ დისტანციურად ან გსურთ შეიძინოთ თქვენთვის",

                ["Product.Reviews.NoReviewsYet"] = "ამ მომენტისთვის შეფასებები არ არის.",
                ["Product.Reviews.WhoCanLeave"] = "მხოლოდ იმ მომხმარებელს შეუძლია შეფასების დაწერა, რომელმაც უკვე გამოცადა ეს სერვიცი.",
                ["Product.Reviews.AddReview"] = "დაამატეთ მიმოხილვა",
                ["Product.Reviews.CTA_AddReview"] = "დაამატეთ მიმოხილვა და დაეხმარეთ ხალხს საუკეთესო არჩევნის გაკეთებაში.",
                ["Product.Reviews.ReedAll"] = "წაიკითხეთ ყველა მიმოხილვ",
                ["Product.Vendor.Contacts"] = "Contacts:",
                ["Product.Vendor.Address"] = "Address:",
                ["Product.Vendor.GetDir"] = "მიიღეთ მიმართულება...",

                ["enums.nop.core.domain.catalog.producttype.multicertificate"] = "მულტისერტიფიკატი",
                ["enums.nop.core.domain.catalog.producttype.PackagingForMulticertificate"] = "მულტისერტიფიკატის შეფუთვა",
                ["enums.nop.core.domain.catalog.producttype.ApplyToMulticertificate"] = "დაამატეთ მულტისერტიფიკატი",
                ["admin.product.producttype.none"] = "არცერთი",

                ["EUCookieLaw.OK"] = "გასაგებია",
            });
        }
    }
}