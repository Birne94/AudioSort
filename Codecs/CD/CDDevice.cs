using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Audiosort.Codecs.CD
{
    public class CDDevice
    {
        IntPtr Handle = IntPtr.Zero;
        char DriveLetter;
        bool Alive = false;
        Win32API.CDROM_TOC TOC;

        public event EventHandler CDInserted;
        public event EventHandler CDRemoved;

        public bool TOCRead = false;

        DeviceChangeNotificationWindow NotificationWindow = null;

        #region constants

        protected const int NSECTORS = 13;
        protected const int UNDERSAMPLING = 1;
        protected const int CB_CDDASECTOR = 2368;
        protected const int CB_QSUBCHANNEL = 16;
        protected const int CB_CDROMSECTOR = 2048;
        protected const int CB_AUDIO = (CB_CDDASECTOR - CB_QSUBCHANNEL);

        #endregion

        /// <summary>
        /// Alle CD-Laufwerke aufzählen
        /// </summary>
        /// <returns>Array mit Laufwerksbuchstaben</returns>
        public static char[] EnumDevices()
        {
            string chars = "";
            for (char c = 'C'; c <= 'Z'; c++)
            {
                if (Win32API.GetDriveType(c + ":") == Win32API.DriveTypes.DRIVE_CDROM)
                    chars += c;
            }
            return chars.ToCharArray();
        }

        /// <summary>
        /// Zugriff auf Laufwerk beginnen
        /// </summary>
        /// <param name="path">Laufwerksbuchstabe</param>
        /// <param name="readTOC">Soll TOC gelesen werden?</param>
        public CDDevice(char path, bool readTOC = true)
        {
            if (Win32API.GetDriveType(path + ":") != Win32API.DriveTypes.DRIVE_CDROM)
                throw new InvalidCD();

            // Device öffnen
            Handle = Win32API.CreateFile(
                "\\\\.\\" + path + ":",
                Win32API.GENERIC_READ, Win32API.FILE_SHARE_READ,
                IntPtr.Zero,
                Win32API.OPEN_EXISTING, 0,
                IntPtr.Zero
                );

            if ((Handle == IntPtr.Zero) || ((int)Handle == -1))
                throw new InvalidCD();

            TOC = new Win32API.CDROM_TOC();
            DriveLetter = path;
            Alive = true;

            NotificationWindow = new DeviceChangeNotificationWindow();
            NotificationWindow.DeviceChange += NoticeDeviceChange;

            if (readTOC)
                ReadTOC();
        }

        ~CDDevice()
        {
            Close();
        }

        /// <summary>
        /// Kommunikation mit Device beenden
        /// </summary>
        public void Close()
        {
            Win32API.CloseHandle(Handle);
            Handle = IntPtr.Zero;
            if (NotificationWindow != null)
                NotificationWindow.DestroyHandle();
            NotificationWindow = null;
            Alive = false;
        }

        /// <summary>
        /// Table of Content lesen
        /// </summary>
        public void ReadTOC()
        {
            if (!Alive)
                throw new InvalidCD();

//            if (!CDReady())
//                throw new NoCD();

            uint bytesRead = 0;
            int result = Win32API.DeviceIoControl(
                Handle,
                Win32API.IOCTL_CDROM_READ_TOC,
                IntPtr.Zero, 0,
                TOC, (uint)Marshal.SizeOf(TOC),
                ref bytesRead,
                IntPtr.Zero
                );

            if (result == 0)
                throw new InvalidTOC();

            TOCRead = true;
        }

        /// <summary>
        /// Startsektor eines Tracks ermitteln
        /// </summary>
        /// <param name="track">Nummer des Tracks</param>
        /// <returns></returns>
        protected int GetStartSector(int track)
        {
            if (!Alive) return -1;
            Win32API.TRACK_DATA toc = TOC.TrackData[track - 1];
            return (toc.Address_1 * 60 * 75 + toc.Address_2 * 75 + toc.Address_3) - 150;
        }

        /// <summary>
        /// Endsektor eines Tracks ermitteln
        /// </summary>
        /// <param name="track">Nummer des Tracks</param>
        /// <returns></returns>
        protected int GetEndSector(int track)
        {
            if (!Alive) return -1;
            Win32API.TRACK_DATA toc = TOC.TrackData[track];
            return (toc.Address_1 * 60 * 75 + toc.Address_2 * 75 + toc.Address_3) - 151;
        }

        /// <summary>
        /// CD-Laufwerk (ent)sperren
        /// </summary>
        /// <param name="unlock">false = lock, true = unlock</param>
        /// <returns>true im Erfolgsfall</returns>
        public bool LockCD(bool unlock = false)
        {
            if (!Alive) return false;

            uint x = 0; // Dummy

            Win32API.PREVENT_MEDIA_REMOVAL pmr = new Win32API.PREVENT_MEDIA_REMOVAL();
            pmr.PreventMediaRemoval = (byte)(unlock ? 0 : 1);
            return Win32API.DeviceIoControl(
                Handle,
                Win32API.IOCTL_STORAGE_MEDIA_REMOVAL, pmr,
                (uint)Marshal.SizeOf(pmr), IntPtr.Zero,
                0, ref x, IntPtr.Zero
                ) != 0;
        }

        /// <summary>
        /// CD auswerfen/einziehen
        /// </summary>
        /// <param name="load">false = eject, true = load</param>
        /// <returns>true im Erfolgsfall</returns>
        public bool EjectCD(bool load = false)
        {
            if (!Alive) return false;

            uint x = 0; // Dummy
            return Win32API.DeviceIoControl(
                Handle,
                load ? Win32API.IOCTL_STORAGE_LOAD_MEDIA : Win32API.IOCTL_STORAGE_EJECT_MEDIA,
                IntPtr.Zero, 0, IntPtr.Zero,
                0, ref x, IntPtr.Zero
                ) != 0;
        }

        /// <summary>
        /// Prüfen, ob eine CD im Laufwerk ist
        /// </summary>
        /// <returns></returns>
        public bool CDReady()
        {
            if (!Alive)
                throw new InvalidCD();

            uint x = 0; // Dummy
            if (Win32API.DeviceIoControl(
                Handle,
                Win32API.IOCTL_STORAGE_CHECK_VERIFY,
                IntPtr.Zero, 0, IntPtr.Zero, 0,
                ref x,
                IntPtr.Zero) != 0)
            {
                return true;
            }

            TOC = null;
            return false;
        }

        /// <summary>
        /// Wenn eine CD im Laufwerk ist, TOC laden
        /// </summary>
        public void Refresh()
        {
            if (CDReady())
                ReadTOC();
            else
                TOC = null;
        }

        /// <summary>
        /// Anzahl CD-Tracks ermitteln
        /// </summary>
        /// <returns></returns>
        public int GetNumTracks()
        {
            if (TOC != null)
                return TOC.LastTrack - TOC.FirstTrack + 1;
            return 0;
        }

        /// <summary>
        /// Anzahl Audio-Tracks ermitteln
        /// </summary>
        /// <returns></returns>
        public int GetNumAudioTracks()
        {
            if (TOC != null)
            {
                int tracks = 0;
                for (int i = TOC.FirstTrack - 1; i < TOC.LastTrack; i++)
                {
                    if (TOC.TrackData[i].Control == 0)
                        tracks++;
                }
                return tracks;
            }
            return 0;
        }

        /// <summary>
        /// Track von CD lesen
        /// </summary>
        /// <param name="track">Tracknummer</param>
        /// <param name="data">Buffer</param>
        /// <param name="dataSize">ref: Gelesene Bytes</param>
        /// <param name="start">Anfang des Tracks in Sekunden (0 = Anfang)</param>
        /// <param name="count">Anzahl zu lesende Sekunden (0 = Alle)</param>
        /// <returns>Gelesene Bytes</returns>
        public int ReadTrack(int track, byte[] data, ref uint dataSize, uint start, uint count)
        {
            if (TOC == null || track < TOC.FirstTrack || track > TOC.LastTrack)
                return -1;

            int SectorStart = GetStartSector(track);
            int SectorStop = GetEndSector(track);

            if (SectorStart + (int)start * 75 < SectorStop)
                SectorStart += (int)start * 75;
            if (count > 0 && (int)(SectorStart + start * 75) < SectorStop)
                SectorStop = SectorStart + (int)count * 75;

            dataSize = (uint)(SectorStop - SectorStart) * CB_AUDIO;
            if (data == null)
                return 0;
            if (data.Length >= dataSize)
            {
                CDBufferFiller filler = new CDBufferFiller(data);
                return ReadTrack(track, new CdDataReadEventHandler(filler.OnDataRead), start, count);
            }
            return 0;
        }

        /// <summary>
        /// Track von CD lesen
        /// </summary>
        /// <param name="track">Tracknummer</param>
        /// <param name="ReadEvent">Buffer</param>
        /// <param name="start">Anfang des Tracks in Sekunden (0 = Anfang)</param>
        /// <param name="count">Anzahl zu lesende Sekunden (0 = Alle)</param>
        /// <returns>Gelesene Bytes</returns>
        public int ReadTrack(int track, CdDataReadEventHandler ReadEvent, uint start, uint count)
        {
            if (TOC == null || track < TOC.FirstTrack || track > TOC.LastTrack)
                return -1;

            int SectorStart = GetStartSector(track);
            int SectorStop = GetEndSector(track);

            if (SectorStart + (int)start * 75 < SectorStop)
                SectorStart += (int)start * 75;
            if (count > 0 && (int)(SectorStart + start * 75) < SectorStop)
                SectorStop = SectorStart + (int)count * 75;

            uint dataSize = (uint)(SectorStop - SectorStart) * CB_AUDIO;
            uint dataRead = 0;
            byte[] data = new byte[CB_AUDIO * NSECTORS];
            bool cont = true;
            bool ok = true;

            for (int sector = SectorStart; (sector < SectorStop) && cont && ok; sector += NSECTORS)
            {
                int ToRead = ((sector + NSECTORS) < SectorStop) ? NSECTORS : (SectorStop - sector);
                ok = ReadSector(sector, data, ToRead);
                if (ok)
                {
                    ReadEvent(this, new DataEventArgs(data, (uint)(CB_AUDIO * ToRead)));
                    dataRead += (uint)(CB_AUDIO * ToRead);

                }
            }

            if (ok)
                return (int)dataRead;
            return -1;
        }

        protected bool ReadSector(int sector, byte[] buff, int count)
        {
            if (TOC == null || (sector + count) > GetEndSector(TOC.LastTrack) || (buff.Length < CB_AUDIO * count))
                return false;

            Win32API.RAW_READ_INFO info = new Win32API.RAW_READ_INFO();
            info.TrackMode = Win32API.TRACK_MODE_TYPE.CDDA;
            info.SectorCount = (uint)count;
            info.DiskOffset = sector * CB_CDROMSECTOR;

            uint dataRead = 0;
            if (Win32API.DeviceIoControl(
                Handle,
                Win32API.IOCTL_CDROM_RAW_READ,
                info, (uint)Marshal.SizeOf(info),
                buff, (uint)count * CB_AUDIO,
                ref dataRead, IntPtr.Zero) != 0)
                return true;
            return false;
        }

        /// <summary>
        /// Größe eines Tracks ermitteln
        /// </summary>
        /// <param name="track">Tracknummer</param>
        /// <returns></returns>
        public uint TrackSize(int track)
        {
            uint size = 0;
            ReadTrack(track, null, ref size, 0, 0);
            return size;
        }

        /// <summary>
        /// Prüfen, ob Track ein Audiotrack ist
        /// </summary>
        /// <param name="track">Tracknummer</param>
        /// <returns></returns>
        public bool isAudioTrack(int track)
        {
            if (TOC == null || track < TOC.FirstTrack || track > TOC.LastTrack)
                return false;

            return (TOC.TrackData[track - 1].Control & 4) == 0;
        }

        /// <summary>
        /// CDDB-ID ermitteln
        /// </summary>
        /// <returns></returns>
        public string GetCDDB1ID()
        {
            int trackCount = GetNumTracks();
            int n = 0;

            double offset = 0;
            int seconds = 0;
            int i;

            for (i = 0; i < trackCount; i++)
            {
                offset = ((TOC.TrackData[i].Address_1 * 60) + TOC.TrackData[i].Address_2) * 75 + TOC.TrackData[i].Address_3;
                n += cross_sum((TOC.TrackData[i].Address_1 * 60) + TOC.TrackData[i].Address_2);
                seconds += GetTrackLength(i);
            }

            int numSeconds = TOC.TrackData[i].Address_1 * 60 + TOC.TrackData[i].Address_2;
            Win32API.TRACK_DATA first = TOC.TrackData[0];
            Win32API.TRACK_DATA last = TOC.TrackData[trackCount];

            int t = ((last.Address_1 * 60) + last.Address_2) - ((first.Address_1 * 60) + first.Address_2);
            uint discId = (((uint)n & 0xff) << 24 | (uint)t << 8 | (uint)trackCount);
            return String.Format("{0:x8}", discId);
        }

        /// <summary>
        /// Quersumme einer Zahl ermitteln
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private int cross_sum(int p)
        {
            int result = 0;
            while (p > 0)
            {
                result += p % 10;
                p /= 10;
            }
            return result;
        }

        /// <summary>
        /// Länge eines Tracks in Sekunden ermitteln
        /// </summary>
        /// <param name="track">Tracknummer</param>
        /// <returns></returns>
        public int GetTrackLength(int track)
        {
            if (TOC == null || track < TOC.FirstTrack || track > TOC.LastTrack)
                return -1;

            int start = (GetStartSector(track) + 150) / 75;
            int stop = (GetEndSector(track) + 150) / 75;

            return stop - start;
        }

        public Win32API.CDROM_TOC GetTOC()
        {
            return TOC;
        }

        #region Events
        private void OnCDInserted()
        {
            if (CDInserted != null)
                CDInserted(this, EventArgs.Empty);
        }
        private void OnCDRemoved()
        {
            if (CDRemoved != null)
                CDRemoved(this, EventArgs.Empty);
        }
        private void NoticeDeviceChange(object sender, DeviceChangeEventArgs e)
        {
            if (e.Drive == DriveLetter)
            {
                TOC = null;
                switch (e.Type)
                {
                    case DeviceChangeEventType.DeviceInserted:
                        OnCDInserted();
                        break;
                    case DeviceChangeEventType.DeviceRemoved:
                        OnCDRemoved();
                        break;
                }
            }
        }
        #endregion
    }
}
