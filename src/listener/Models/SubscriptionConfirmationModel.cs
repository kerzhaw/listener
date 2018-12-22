/*
{
  "Type" : "SubscriptionConfirmation",
  "MessageId" : "165545c9-2a5c-472c-8df2-7ff2be2b3b1b",
  "Token" : "2336412f37fb687f5d51e6e241d09c805a5a57b30d712f794cc5f6a988666d92768dd60a747ba6f3beb71854e285d6ad02428b09ceece29417f1f02d609c582afbacc99c583a916b9981dd2728f4ae6fdb82efd087cc3b7849e05798d2d2785c03b0879594eeac82c01f235d0e717736",
  "TopicArn" : "arn:aws:sns:us-west-2:123456789012:MyTopic",
  "Message" : "You have chosen to subscribe to the topic arn:aws:sns:us-west-2:123456789012:MyTopic.\nTo confirm the subscription, visit the SubscribeURL included in this message.",
  "SubscribeURL" : "https://sns.us-west-2.amazonaws.com/?Action=ConfirmSubscription&TopicArn=arn:aws:sns:us-west-2:123456789012:MyTopic&Token=2336412f37fb687f5d51e6e241d09c805a5a57b30d712f794cc5f6a988666d92768dd60a747ba6f3beb71854e285d6ad02428b09ceece29417f1f02d609c582afbacc99c583a916b9981dd2728f4ae6fdb82efd087cc3b7849e05798d2d2785c03b0879594eeac82c01f235d0e717736",
  "Timestamp" : "2012-04-26T20:45:04.751Z",
  "SignatureVersion" : "1",
  "Signature" : "EXAMPLEpH+DcEwjAPg8O9mY8dReBSwksfg2S7WKQcikcNKWLQjwu6A4VbeS0QHVCkhRS7fUQvi2egU3N858fiTDN6bkkOxYDVrY0Ad8L10Hs3zH81mtnPk5uvvolIC1CXGu43obcgFxeL3khZl8IKvO61GWB6jI9b5+gLPoBc1Q=",
  "SigningCertURL" : "https://sns.us-west-2.amazonaws.com/SimpleNotificationService-f3ecfb7224c7233fe7bb5f59f96de52f.pem"
  }
 */
using System;
using System.ComponentModel.DataAnnotations;

namespace listener.Models
{
    public class SubscriptionConfirmationModel
    {
        /// <summary>
        /// A string that describes the message. (remember to de-escape)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Message { get; set; }

        /// <summary>
        /// A Universally Unique Identifier, unique for each message published. 
        /// For a message that Amazon SNS resends during a retry, the message ID 
        /// of the original message is used.
        /// </summary>
        [Required]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Base64-encoded "SHA1withRSA" signature of the Message, MessageId, 
        /// Type, Timestamp, and TopicArn values.
        /// <value></value>
        [Required(AllowEmptyStrings = false)]
        public string Signature { get; set; }

        /// <summary>
        /// Version of the Amazon SNS signature used.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string SignatureVersion { get; set; }

        /// <summary>
        /// The URL to the certificate that was used to sign the message.
        /// </summary>
        [Required]
        public Uri SigningCertURL { get; set; }

        /// <summary>
        /// The URL that you must visit in order to confirm the subscription.
        /// Alternatively, you can instead use the Token with the 
        /// ConfirmSubscription action to confirm the subscription.
        /// </summary>
        [Required]
        public Uri SubscribeURL { get; set; }

        /// <summary>
        /// The time (GMT) when the subscription confirmation was sent.
        /// </summary>
        [Required]
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// A value you can use with the ConfirmSubscription action to confirm 
        /// the subscription. Alternatively, you can simply visit the 
        /// SubscribeURL.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Token { get; set; }

        /// <summary>
        /// The Amazon Resource Name (ARN) for the topic that this endpoint is 
        /// subscribed to.
        /// </summary>
        /// <value></value>
        [Required(AllowEmptyStrings = false)]
        public string TopicArn { get; set; }

        /// <summary>
        /// The type of message. For a subscription confirmation, the type is 
        /// SubscriptionConfirmation.
        /// </summary>
        /// <value></value>
        [Required(AllowEmptyStrings = false)]
        public string Type { get; set; }
    }
}