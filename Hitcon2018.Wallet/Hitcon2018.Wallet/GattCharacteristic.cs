using System;
using Plugin.BluetoothLE;

namespace Hitcon2018.Wallet
{
    public class GattCharacteristic : AbstractGattCharacteristic
    {
        private IGattCharacteristic chs;

        public GattCharacteristic(IGattCharacteristic chs) : base(chs.Service, chs.Uuid, chs.Properties)
        {
            this.chs = chs;
        }

        public GattCharacteristic(IGattService service, Guid uuid, CharacteristicProperties properties) : base(service, uuid, properties)
        {
        }

        public override byte[] Value => throw new NotImplementedException();

        public override IObservable<CharacteristicGattResult> DisableNotifications()
        {
            throw new NotImplementedException();
        }

        public override IObservable<IGattDescriptor> DiscoverDescriptors()
        {
            throw new NotImplementedException();
        }

        public override IObservable<CharacteristicGattResult> EnableNotifications(bool enableIndicationsIfAvailable)
        {
            return chs.EnableNotifications();
        }

        public override IObservable<CharacteristicGattResult> Read()
        {
            return chs.Read();
        }

        public override IObservable<CharacteristicGattResult> WhenNotificationReceived()
        {
            return chs.WhenNotificationReceived();
        }

        public override IObservable<CharacteristicGattResult> Write(byte[] value)
        {
            return chs.Write(value);
        }

        public override IObservable<CharacteristicGattResult> WriteWithoutResponse(byte[] value)
        {
            throw new NotImplementedException();
        }
    }
}