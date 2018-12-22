/*
POST / HTTP/1.1
x-amz-sns-message-type: Notification
x-amz-sns-message-id: 22b80b92-fdea-4c2c-8f9d-bdfb0c7bf324
x-amz-sns-topic-arn: arn:aws:sns:us-west-2:123456789012:MyTopic
x-amz-sns-subscription-arn: arn:aws:sns:us-west-2:123456789012:MyTopic:c9135db0-26c4-47ec-8998-413945fb5a96
Content-Length: 773
Content-Type: text/plain; charset=UTF-8
Host: myhost.example.com
Connection: Keep-Alive
User-Agent: Amazon Simple Notification Service Agent

{
  "Type" : "Notification",
  "MessageId" : "22b80b92-fdea-4c2c-8f9d-bdfb0c7bf324",
  "TopicArn" : "arn:aws:sns:us-west-2:123456789012:MyTopic",
  "Subject" : "My First Message",
  "Message" : "Hello world!",
  "Timestamp" : "2012-05-02T00:54:06.655Z",
  "SignatureVersion" : "1",
  "Signature" : "EXAMPLEw6JRNwm1LFQL4ICB0bnXrdB8ClRMTQFGBqwLpGbM78tJ4etTwC5zU7O3tS6tGpey3ejedNdOJ+1fkIp9F2/LmNVKb5aFlYq+9rk9ZiPph5YlLmWsDcyC5T+Sy9/umic5S0UQc2PEtgdpVBahwNOdMW4JPwk0kAJJztnc=",
  "SigningCertURL" : "https://sns.us-west-2.amazonaws.com/SimpleNotificationService-f3ecfb7224c7233fe7bb5f59f96de52f.pem",
  "UnsubscribeURL" : "https://sns.us-west-2.amazonaws.com/?Action=Unsubscribe&SubscriptionArn=arn:aws:sns:us-west-2:123456789012:MyTopic:c9135db0-26c4-47ec-8998-413945fb5a96"
}
 */
using System;

namespace listener.Models
{
    public class NotificationModel
    {
        public const string AwsSnsMessageTypeHeaderValue = "Notification";

        /// <summary>
        /// A string that describes the message. (remember to de-escape)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A Universally Unique Identifier, unique for each message published. 
        /// For a message that Amazon SNS resends during a retry, the message ID 
        /// of the original message is used.
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// Base64-encoded "SHA1withRSA" signature of the Message, MessageId, 
        /// Type, Timestamp, and TopicArn values.
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Version of the Amazon SNS signature used.
        /// </summary>
        public string SignatureVersion { get; set; }

        /// <summary>
        /// The URL to the certificate that was used to sign the message.
        /// </summary>
        public Uri SigningCertURL { get; set; }

        /// <summary>
        /// The Subject parameter specified when the notification was published 
        /// to the topic. Note that this is an optional parameter. If no Subject 
        /// was specified, then this name/value pair does not appear in this 
        /// JSON document.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The time (GMT) when the subscription confirmation was sent.
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// The Amazon Resource Name (ARN) for the topic that this endpoint is 
        /// subscribed to.
        /// </summary>
        public string TopicArn { get; set; }

        /// <summary>
        /// The type of message. For a subscription confirmation, the type is 
        /// SubscriptionConfirmation.
        /// </summary>
        public string Type { get; set; }
    }
}