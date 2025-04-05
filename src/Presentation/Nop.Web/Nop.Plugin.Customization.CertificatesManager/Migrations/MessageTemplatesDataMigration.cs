using System;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Services.Customization.Messages;
using Nop.Services.Messages;

namespace Nop.Plugin.Customization.CertificatesManager.Data;

[NopSchemaMigration("2024-08-02 00:01:04", "Add message templates to support Certificates Processing2", MigrationProcessType.NoMatter)]
public class MessageTemplatesDataMigration : Migration
{
    private readonly INopDataProvider _dataProvider;
    private readonly IEmailAccountService _emailAccountService;

    public MessageTemplatesDataMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!_dataProvider.GetTable<MessageTemplate>().Any(mt =>
            string.Compare(mt.Name, CustomMessageTemplateNames.CertificateActivatedStoreOwnerNotification, StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            var activated_storeOwnerNotification = _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = CustomMessageTemplateNames.CertificateActivatedStoreOwnerNotification,
                    Subject = "%Store.Name%. A new Certificate requires an Activation processing",
                    Body = "<p>%Certificate.allInfo%</p>",
                    EmailAccountId = 0,
                    IsActive = true,
                }
            );
        }

        if (!_dataProvider.GetTable<MessageTemplate>().Any(mt =>
            string.Compare(mt.Name, CustomMessageTemplateNames.CertificateProcessingErrorStoreOwnerNotification, StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            var activated_storeOwnerNotification = _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = CustomMessageTemplateNames.CertificateProcessingErrorStoreOwnerNotification,
                    Subject = "%Store.Name% - URGENT! An error happened during multicertificate processing",
                    Body = "<p>%Message%</p>",
                    EmailAccountId = 0,
                    IsActive = true,
                }
            );
        }

        if (!_dataProvider.GetTable<MessageTemplate>().Any(mt =>
            string.Compare(mt.Name, CustomMessageTemplateNames.CertificateProcessingCustomerNotificationECertificateSent, StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            var activated_storeOwnerNotification = _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = CustomMessageTemplateNames.CertificateProcessingCustomerNotificationECertificateSent,
                    Subject = "%Store.Name% - Order #%Order.OrderNumber%: Multicertificate for %Recipient.Name% is sent.",
                    Body = "<p>Dear %Customer.Name%,</p>\r\n<p>By your request for order #%Order.OrderNumber% we generated electronic Multicertificate and sent it to %Recipient.Name% (%Recipient.Email%).</p>\r\n<div>Thank you for choosing&nbsp;us.</div>\r\n<div>With best regards, <a style=\"color: #ff2976; text-decoration: none;\" href=\"%Store.URL%\" target=\"_blank\" rel=\"noopener\">%Store.Name%</a></div>",
                    EmailAccountId = 0,
                    IsActive = true,
                }
            );
        }

        if (!_dataProvider.GetTable<MessageTemplate>().Any(mt =>
            string.Compare(mt.Name, CustomMessageTemplateNames.CertificateProcessingCustomerNotificationECertificateScheduledToSend, StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            var activated_storeOwnerNotification = _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = CustomMessageTemplateNames.CertificateProcessingCustomerNotificationECertificateScheduledToSend,
                    Subject = "%Store.Name% -  Order #%Order.OrderNumber%: Multicertificate for %Recipient.Name% is scheduled to send.",
                    Body = "<p>Dear %Customer.Name%,</p>\r\n<p>We generated electronic Multicertificate for order #%Order.OrderNumber%, and as you requested, we scheduled it's sending to %Recipient.Name% (%Recipient.Email%) on %ScheduledDate%.<br />We will notify you one more time once it will be sent to recipient.</p>\r\n<div style=\"line-height: 1.4;\">Thank you for choosing&nbsp;us.</div>\r\n<div style=\"line-height: 1.4;\">With best regards, <a style=\"color: #ff2976; text-decoration: none;\" href=\"%Store.URL%\" target=\"_blank\" rel=\"noopener\">%Store.Name%</a></div>",
                    EmailAccountId = 0,
                    IsActive = true,
                }
            );
        }

        if (!_dataProvider.GetTable<MessageTemplate>().Any(mt =>
            string.Compare(mt.Name, CustomMessageTemplateNames.CertificateProcessingElectronicCertificateToRecipient, StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            var activated_storeOwnerNotification = _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = CustomMessageTemplateNames.CertificateProcessingElectronicCertificateToRecipient,
                    Subject = "%Store.Name% - %if(!%Certificate.IsAnonym%)%Certificate.SenderName% sent you a gift. endif% %if(%Certificate.IsAnonym%)You've got a gift multicertificate. endif%",
                    Body = "<table style=\"width: 100%; max-width: 100%; margin: 0 auto;\" border=\"0\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\">\r\n<tbody>\r\n<tr>\r\n<td style=\"padding: 0;\" align=\"center\">\r\n<table style=\"font-size: 16px; color: #333868; width: 320px; min-width: 320px; max-width: 320px; overflow-x: auto; margin: 0 auto; border: 1px solid #333868; background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0px 2px 5px rgba(51, 56, 104, 0.5); border-color: rgba(51, 56, 104, 0.3);\" border=\"0\" width=\"320\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"padding: 0; margin: 0;\"><img style=\"width: 320px; min-width: 320px; max-width: 320px; height: auto; display: block; border: 0; margin: 0; padding: 0; border-radius: 16px 16px 0 0;\" src=\"https://hoho.ge/images/static/header.png\" alt=\"header image with logo\" width=\"320\" height=\"auto\" /></td>\r\n</tr>\r\n<tr>\r\n<td style=\"padding: 0 15px; font-size: 16px;\">%if(%Certificate.HasMessage%)\r\n<p style=\"margin-top: 10px; margin-bottom: 5px;\"><strong>%Certificate.RecipientFirstName%,</strong></p>\r\n<p style=\"margin: 0;\">%Certificate.Message%</p>\r\n<p style=\"text-align: right; margin-top: 8px; border-bottom: 1px solid #FF2976; margin-bottom: 0; padding-bottom: 10px;\">with best regards, <strong>%if(!%Certificate.IsAnonym%)%Certificate.SenderName%endif% %if(%Certificate.IsAnonym%)Anonym endif%</strong></p>\r\nendif% %if(!%Certificate.HasMessage%)\r\n<p style=\"margin-top: 10px; margin-bottom: 5px;\"><strong>%Certificate.RecipientFirstName%,</strong></p>\r\n<p style=\"margin: 0;\">We wish you enjoy every moment of your life because you deserve it.</p>\r\n<p style=\"text-align: right; margin-top: 8px; border-bottom: 1px solid #FF2976; margin-bottom: 0; padding-bottom: 10px;\">with best regards, <strong>%Store.Name%</strong></p>\r\nendif%</td>\r\n</tr>\r\n<tr>\r\n<td style=\"padding: 15px 15px 0 15px;\">\r\n<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"width: 115px; height: 115px; border: 2px solid #BFD613; border-radius: 12px;\"><img style=\"width: 100%; height: auto; display: block; border: 0; margin: 0; padding: 0; border-radius: 12px;\" src=\"%Certificate.QRCodeUrl%\" alt=\"image with QR code to scan\" width=\"320\" height=\"auto\" /></td>\r\n<td style=\"padding-left: 10px;\">\r\n<p style=\"margin: 0; font-size: 15px;\"><strong>სერტიფიკატის ნომერი</strong></p>\r\n<p style=\"margin: 5px 0; font-size: 16px;\">%Certificate.CardNumber%</p>\r\n<p style=\"margin: 0; font-size: 15px; margin-top: 10px;\"><strong>დაცვის კოდი*</strong></p>\r\n<p style=\"margin: 5px 0; font-size: 16px;\">%Certificate.PinCode%</p>\r\n</td>\r\n</tr>\r\n</tbody>\r\n</table>\r\n</td>\r\n</tr>\r\n<tr>\r\n<td style=\"padding: 0 15px; color: #ff9d29;\">\r\n<p style=\"margin: 0; font-size: 12px;\">*არ გაუზიაროთ კოდი სხვა პირებს</p>\r\n</td>\r\n</tr>\r\n<tr>\r\n<td style=\"padding: 15px;\"><a style=\"display: block; margin: 0 auto; width: 160px; text-align: center; padding: 8px 24px; font-family: Arial, sans-serif; font-size: 18px; color: #ffffff; background-color: #333868; text-decoration: none; border-radius: 8px;\" href=\"%Certificate.RedeemUrl%\" target=\"_blank\" rel=\"noopener\"> <span style=\"margin: 0;\">ნახეთ რა აჩუქა</span> <span style=\"margin: 0; font-size: 12px; display: block; width: 100%; text-align: center;\">View what gifted</span> </a></td>\r\n</tr>\r\n<tr>\r\n<td style=\"padding: 0px 15px 0 15px;\">\r\n<table style=\"width: 100%;\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n<tbody>\r\n<tr>\r\n<td>\r\n<p style=\"margin: 0; font-size: 14px;\">მოქმედების ბოლო თარიღია:</p>\r\n<p style=\"margin: 0; font-size: 14px;\">Expires at:</p>\r\n</td>\r\n<td>\r\n<p style=\"font-size: 16px; text-align: right; margin: 0;\">%Certificate.ExpiresAt%</p>\r\n</td>\r\n</tr>\r\n</tbody>\r\n</table>\r\n</td>\r\n</tr>\r\n<tr>\r\n<td style=\"padding: 0 5px 10px; margin: 0 auto;\"><a style=\"text-align: center; color: #ff2976; text-decoration: none; width: 100%; display: block; border-top: 1px solid #FF2976; margin-top: 10px;\" href=\"%Store.URL%\" target=\"_blank\" rel=\"noopener\">hoho.ge</a></td>\r\n</tr>\r\n</tbody>\r\n</table>\r\n</td>\r\n</tr>\r\n</tbody>\r\n</table>",
                    EmailAccountId = 0,
                    IsActive = true,
                }
            );
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}
