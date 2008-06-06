namespace MassTransit.ServiceBus.Tests.Formatters
{
    using System.IO;
    using System.Text;
    using MassTransit.ServiceBus.Formatters;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;
    using Rhino.Mocks;

    public class BinaryBodyFormatterTests :
        Specification
    {
        private MockRepository mocks;
        private BinaryBodyFormatter formatter;
        private IFormattedBody mockBody;

        private readonly byte[] _serializedMessages = new byte[161];

        private readonly string _serializedMessagesWithValue =
             @"\0\0\0\0����\0\0\0\0\0\0\0\f\0\0\0SMassTransit.ServiceBus.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\0\0\0*MassTransit.ServiceBus.Tests.ClientMessage\0\0\0_name\0\0\0\0\0\0test\v";

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            formatter = new BinaryBodyFormatter();
            mockBody = mocks.CreateMock<IFormattedBody>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks = null;
            formatter = null;
            mockBody = null;
        }

        [Test]
        [Ignore]
        public void Serialize()
        {
            PingMessage msg = new PingMessage();
            MemoryStream ms = new MemoryStream();

            using (mocks.Record())
            {
                Expect.Call(mockBody.BodyStream).Return(ms);
            }

            using (mocks.Playback())
            {
                formatter.Serialize(mockBody, msg);
            }
            
            Assert.That(ms.Length, Is.EqualTo(161));
        }

        [Test]
        [Ignore]
        public void Deserialize()
        {
            MemoryStream ms = new MemoryStream(_serializedMessages);
            using (mocks.Record())
            {
                Expect.Call(mockBody.BodyStream).Return(ms);
            }
            using (mocks.Playback())
            {
                PingMessage msg = formatter.Deserialize<PingMessage>(mockBody);

                Assert.IsNotNull(msg);

                Assert.That(msg, Is.TypeOf(typeof(PingMessage)));
            }
        }

        [Test]
        [Ignore]
        public void DeserializeWithOutGenerics()
        {
            MemoryStream ms = new MemoryStream(_serializedMessages);
            using (mocks.Record())
            {
                Expect.Call(mockBody.BodyStream).Return(ms);
            }
            using (mocks.Playback())
            {
                object msg = formatter.Deserialize(mockBody);

                Assert.IsNotNull(msg);

                Assert.That(msg, Is.TypeOf(typeof(PingMessage)));
            }
        }

        [Test]
        [Ignore]
        public void SerializeObjectWithValues()
        {
            ClientMessage msg = new ClientMessage();
            msg.Name = "test";

            MemoryStream ms = new MemoryStream();
            using (mocks.Record())
            {
                Expect.Call(mockBody.BodyStream).Return(ms);
            }

            using (mocks.Playback())
            {
                formatter.Serialize(mockBody, msg);
            }

            byte[] buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, (int)ms.Length);
            ms.Flush();

            string s = Encoding.UTF8.GetString(buffer);

        }

        [Test]
        [Ignore]
        public void DeserializeObjectWithValues()
        {
            ClientMessage msg = new ClientMessage();
            msg.Name = "test";

            MemoryStream ms = new MemoryStream();
            byte[] buffer = Encoding.UTF8.GetBytes(_serializedMessagesWithValue);
            ms.Read(buffer, 0, buffer.Length);
            ms.Flush();
            ms.Position = 0;

            object actual;
            using (mocks.Record())
            {
                Expect.Call(mockBody.BodyStream).Return(ms);
            }

            using (mocks.Playback())
            {
                actual = formatter.Deserialize(mockBody);
            }

            Assert.IsInstanceOfType(typeof(ClientMessage), actual);

        }
    }
}