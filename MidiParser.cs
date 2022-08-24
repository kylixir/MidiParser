using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using UnityEngine;



namespace MidiParser
{

    public abstract class ExtensibleMidiEvents
    {
        //The most commonly used event in MIDI. Triggers anytime a note is intended to be played. Most use cases for this program will only need to extend this single event.
        public virtual void NoteOn(int timeMS, int track, int channel, int note, int velocity) { }

        //Rarely used in MIDI! Typically you will see a "Note On" event with a velocity (volume) of zero instead of a Note Off event.
        public virtual void NoteOff(int timeMS, int track, int channel, int note, int velocity) { }

        //Modifies a single note after it has been played. Typically the pressure will modify the Velocity; less commonly the Pitch of the note. It can be used for any purpose however.
        public virtual void PolyphonicPressure(int timeMS, int track, int channel, int note, int pressure) { }

        // http://www.somascape.org/midi/tech/spec.html#ctrlnums
        // See the above link. Implementation of Control Change events does --NOT-- require every possible Selection Value to be handled.
        // If you implement any of the "High Resolution Controllers" you MUST implement the MSB and LSB versions. The MSB must be followed by the LSB in the midi file, and you should throw an error if it is not.
        public virtual void ControlChange(int timeMS, int track, int channel, int controlSelectionValue, int value) { }

        //Specifies the "program" (typically instrument or sample).
        public virtual void ProgramChange(int timeMS, int track, int channel, int selectedProgram) { }

        //Applies PolyphonicPressure on all notes (0-127) for a single channel.
        public virtual void ChannelPolyphonicPressure(int timeMS, int track, int channel, int pressure) { }

        //Defines the change in pitch for all notes on a single channel. Bend Amount is a value (0-32767) with 16384 being the "No pitch bend" value. 0 is the max bend downward, and 32767 is the max bend upwards.
        public virtual void PitchBend(int timeMS, int track, int channel, int bendAmount) { }

        

        //All of the below functions are system messages, and do not strictly relate to MIDI data. All events below are OPTIONAL and are potentially unused even in midi files that would assumedly use them.
         

        //The key signature of a track at a certain time. The range is -14 to 14, negative indicates number of flats and positive indicates sharps. "isMajorKey" is True if in a major key, and False if in a minor key.
        public virtual void KeySignature(int timeMS, int track, int keySignatureSharps, bool isMajorKey) { }

        //An identifying number given to tracks, typically later refferenced in a midi cue message. A jukebox for example may use a midi cue message to play tracks, or even entire files.
        public virtual void SequenceNumber(int timeMS, int sequenceValue) { }

        //Used to indicate a delay at the beginning before any midi data should be read or played. Extremely rarely used.
        public virtual void SMPTE_Offset(int timeMS, int hours, int minutes, int seconds, int frames, int fractionalFrames, int frameRate, bool hasDropFrames) { }

        //Specifies which port the midi data should be sent through, used to circumvent the low 16 MIDI channel limit.
        public virtual void MidiPort(int timeMS, int track, int portNumber) { }

        //The numerator and denominator for a time signature at a given time. Midi clocks per Metronome pulse determines how often the metronome should pulse - a quarter note is always 24 midi clocks, so a value of 12 would mean a pulse every eigth note.
        //The parameter "32nd notes per quarter note" is as bizarre as it sounds. Almost always this value is 8, but some sequencers can have it be other values to change what the midi thinks a quarter note is. Best of luck to you if this isn't 8.
        public virtual void TimeSignature(int timeMS, int timeSignatureNumerator, int timeSignatureDenominator, int midiClocksPerMetronomePulse, int thirtySecondNotesPerQuarterNote) { }


        /*
         * WARNING! The below functions are susceptible to memory underflow attacks if the file being parsed has a longer reported size than the file actually is.
         * In the worst case scenario, carelessly implementing the byte array functions could lead to arbitrary code execution exploits. The text functions may result in unauthorized access of arbitrary computer memory.
         * Poor implementation of the byte array fucntions may result in arbitrary code execution even without an underflow attack. Plan for malicious MIDI data!
         * If you did not understand any of the above, I highly reccomend you DO NOT extend the below functions. I am not responsible for any damage that results from the use of this code.
         */


        //Text for comments, information, and general usage.
        public virtual void Text(int timeMS, int track, string text) { }

        //Text describing the copyright details. There is normally only one event of this type at time 0.
        public virtual void Copyright(int timeMS, int track, string text) { }

        //Provides the name of the track. Normally occurs at time 0.
        public virtual void TrackName(int timeMS, int track, int channel, string text) { }

        //Provides the name of the instrument on a given track and channel. Can happen at any time.
        public virtual void InstrumentName(int timeMS, int track, int channel, string text) { }

        //The lyrics of the track. If the file has lyrics, this event will likely be called many times.
        public virtual void Lyric(int timeMS, int track, int channel, string text) { }

        //Text that typically serves some music or performance helper such as "First Chorus" or "Change Instruments".
        public virtual void Marker(int timeMS, int track, int channel, string text) { }

        //Text that typically serves some visual or performance helper such as "Change Stage Lights" or "Actor enters from the left"
        public virtual void CuePoint(int timeMS, int track, int channel, string text) { }

        //This text and event is usually used after a Bank/Program change to indicate the instrument or function of the selected program, typically for display on a hardware device.
        public virtual void ProgramName(int timeMS, int track, int channel, string text) { }

        //Used to identify the hardware device that was used to make this tracks sounds.
        public virtual void DeviceName(int timeMS, int track, string text) { }


        //Advanced system events below, see http://www.somascape.org/midi/tech/spec.html for a good starting place if you wish the implement the below functions.

        //A system meta event that does not have official or common use documentation. Only extend this function if you have a custom MIDI implementation you plan to support.
        public virtual void UnknownMetaEvent(int timeMS, int track, byte[] dataArray, int arrayLength) { }

        //Typically used to implement the "Real Time" Midi directions that cannot be included in a midi file, but can theoretically be used for any arbitrary byte array.
        public virtual void SystemEscapeMetaEvent(int timeMS, int track, byte[] dataArray, int arrayLength) { }

        //A byte stream with manufacturer specific functionality. The breadth of this event is beyond the scope of this parser. Some manufacturer specifications are open, many are closed. I wish you the best of luck if you want to implement this.
        //If the "is unfinished message" flag is set to true, then there should be more bytes to follow. This parser does not throw an error if a system message never finishes.
        public virtual void SystemMessage(int timeMS, int track, byte[] dataArray, int arrayLength, bool isUnfinishedMessage) { }

        //A truly useless event that can only occur on Cassio systems, or custom MIDI implementations that use Empty System Messages. It is included here exclusively for the latter purpose.
        public virtual void EmptySystemMessage(int timeMS, int track) { }

        //Similar to the System Message, a byte stream with manufacturer specific functionality. Thankfully, this event includes either 1 or 3 bytes upfront identifying the manufacturer.
        public virtual void SequencerSpecificEvent(int timeMS, int track, byte[] dataArray, int arrayLength) { }


    }

    public enum ErrorType
    {
        BadMidiFile,
        UnsupportedMidi
    }

    enum MidiDefs : int
    {
        MidiHeaderIdentifier = 0x4d546864,
        TrackHeaderIdentifier = 0x4d54726b,

        MidiChannelBits = 0x0F,
        MidiStatusEventBits = 0xF0,

        SPMTE_IdentifierBits = 0x8000,
        SMPTE_FPSBits = 0xFF00,
        SMPTE_ResolutionBits = 0x00FF,
        MetricalDataBits = 0x7FFF,

        UnicodeIdentifierBits = 0xF0,

        VariableIntContinuationFlag = 0x80,
        VariableIntDataBits = 0x7F,

        KeySignatureMajorFlag = 0x00,

        RunningStatusIDMax = 0x7F,
        NoteOnIdentifier = 0x90,
        NoteOffIdentifier = 0x80,
        PolyphonicPressureIdentifier = 0xA0,
        ControllerChangeIdentifier = 0xB0,
        ProgramChangeIdentifier = 0xC0,
        ChannelPressureIdentifier = 0xD0,
        PitchBendIdentifier = 0xE0,
        SystemMessageIdentifier = 0xF0,

        SysexMessageIdentifier = 0xF0,
        SysexEscapeIdentifier = 0xF7,
        MetaEventIdentifier = 0xFF,

        SequenceNumberIdentifier = 0x00,
        TextIdentifier = 0x01,
        CopyrightIdentifier = 0x02,
        TrackNameIdentifier = 0x03,
        InstrumentNameIdentifier = 0x04,
        LyricIdentifier = 0x05,
        MarkerIdentifier = 0x06,
        CuePointIdentifier = 0x07,
        ProgramNameIdentifier = 0x08,
        DeviceNameIdentifier = 0x09,
        MidiChannelPrefixIdentifier = 0x20,
        MidiPortIdentifier = 0x21,
        EndOfTrackIdentifier = 0x2F,
        TempoIdentifier = 0x51,
        SMPTEOffsetIdentifier = 0x54,
        TimeSignatureIdentifier = 0x58,
        KeySignatureIdentifier = 0x59,
        SequencerSpecificEventIdentifier = 0x7F
    }



    public class MidiFileException : Exception
    {
        ErrorType errorType;
        public MidiFileException() { }
        public MidiFileException(string message) : base(message) { }
        public MidiFileException(string message, Exception inner) : base(message, inner) { }
        public MidiFileException(string message, ErrorType type) : base(message)
        {
            errorType = type;
        }
    }

    public class TimeCode
    {
        public class TempoDescriptor
        {
            public int microsecondsPerQuarterNote;
            public int tickTimePosition;
            public TempoDescriptor(int MSPQN, int tickTimePos)
            {
                microsecondsPerQuarterNote = MSPQN;
                tickTimePosition = tickTimePos;
            }
        }
        private enum TimeCodeStyle
        {
            Metrical,
            PerSecond
        }

        private List<TempoDescriptor> tempoList;
        TimeCodeStyle style;
        bool isInError = false;

        int framesPerQuarterNote = 0; // Used for Metrical time.
        float miliSecondsPerTick = 0; // User for SMPTE (per second) time.


        public int TickTimeToMiliSeconds(int tickTime)
        {
            if (style == TimeCodeStyle.PerSecond)
            {
                return (int)Math.Floor(miliSecondsPerTick * tickTime);
            }

            else //time code is metrical
            {
                if(tempoList.Count == 0)
                {
                    return 0;
                }


                int ticksInBlock = 0;
                int timeMicroSecondsPerTick = 0;
                int microseconds = 0;
                int miliseconds = 0;


                int runningTime = 0;
                for (int i = 0; i < tempoList.Count - 1; i++)
                {
                    if (tempoList[i + 1].tickTimePosition <= tickTime) //time to be translated is in a tempo section further ahead
                    {
                        ticksInBlock = (tempoList[i + 1].tickTimePosition - tempoList[i].tickTimePosition);
                        timeMicroSecondsPerTick = (tempoList[i].microsecondsPerQuarterNote / framesPerQuarterNote);
                        microseconds = ticksInBlock * timeMicroSecondsPerTick;
                        miliseconds = microseconds / 1000;
                        runningTime += miliseconds;
                    }
                    else //time to be translated is in this tempo section
                    {
                        ticksInBlock = (tickTime - tempoList[i].tickTimePosition);
                        timeMicroSecondsPerTick = (tempoList[i].microsecondsPerQuarterNote / framesPerQuarterNote);
                        microseconds = ticksInBlock * timeMicroSecondsPerTick;
                        miliseconds = microseconds / 1000;
                        runningTime += miliseconds;
                        return runningTime;
                    }
                }
                //time to be translated is in the final tempo section, and we have exited the loop to prevent out of bounds access
                ticksInBlock = (tickTime - tempoList.Last().tickTimePosition);
                timeMicroSecondsPerTick = (tempoList.Last().microsecondsPerQuarterNote / framesPerQuarterNote);
                microseconds = ticksInBlock * timeMicroSecondsPerTick;
                miliseconds = microseconds / 1000;
                runningTime += miliseconds;
                return runningTime;
            }
        }

        public void AddTempo(int microsecondsPerQuarterNote, int currentTickTime)
        {
            foreach (TempoDescriptor tempoDesc in tempoList)
            {
                if (currentTickTime <= tempoDesc.tickTimePosition)
                {
                    isInError = true;
                    return;
                }
            }
            TempoDescriptor newTempo = new TempoDescriptor(microsecondsPerQuarterNote, currentTickTime);
            tempoList.Add(newTempo);
        }

        public List<TempoDescriptor> getTempos()
        {
            return tempoList;
        }

        public TimeCode(int dataBytes)
        {
            tempoList = new List<TempoDescriptor>();
            if ((dataBytes & (int)MidiDefs.SPMTE_IdentifierBits) == (int)MidiDefs.SPMTE_IdentifierBits)
            {
                style = TimeCodeStyle.PerSecond;
                int framesPerSecond = 0;
                int SPMTESpecification = (dataBytes & (int)MidiDefs.SMPTE_FPSBits);
                int subResolution = (dataBytes & (int)MidiDefs.SMPTE_ResolutionBits);
                switch (SPMTESpecification)
                { //The below Hex values are specified by the SMPTE standard for the relevant frames per second.
                    case 0xE8:
                        framesPerSecond = 24 * subResolution;
                        break;
                    case 0xE7:
                        framesPerSecond = 25 * subResolution;
                        break;
                    case 0xE3:
                        framesPerSecond = 29 * subResolution;
                        break;
                    case 0xE2:
                        framesPerSecond = 30 * subResolution;
                        break;
                    default: //Only the above four frame rates are supported by SMPTE.
                        framesPerSecond = -1;
                        isInError = true;
                        break;
                }
                miliSecondsPerTick = (1000 / framesPerSecond);
            }
            else
            {
                style = TimeCodeStyle.Metrical;
                framesPerQuarterNote = (dataBytes & (int)MidiDefs.MetricalDataBits);
            }
        }

        public bool isError() { return isInError; }
    }

    public class MidiParser
    {
        long fileDataCursor = 0;
        byte[] fileData;
        long totalFileSize = 0;
        int deltaTickTime = 0;
        int currentTickTime = 0;
        int previousStatusCode = -1;
        int statusCode = 0;
        int midiFormat = 0;
        int numberOfTracks = 0;
        int currentTrackNumber = 1;
        int currentTrackLength = 0;
        int currentTrackPosition = 0;
        int sysExChannel = 0;
        bool isUnfinishedSysexMessage = false;
        bool isFinishedParsing = false;
        ExtensibleMidiEvents customImpl;
        TimeCode timeCode;

        public MidiParser(string AbsoluteFilePath, ExtensibleMidiEvents impl)
        {
            customImpl = impl;
            try
            {
                FileInfo fileInfo = new FileInfo(AbsoluteFilePath);
                totalFileSize = fileInfo.Length;
                fileData = new byte[totalFileSize];
                fileData = File.ReadAllBytes(AbsoluteFilePath);
            }
            catch (Exception ex)
            {
                throw new MidiFileException("Could not read file! Please make sure the file exists and has read permissions.", ex);
            }
        }

        public void ParseFile()
        {
            //Begin parsing file header information.
            int headerChunkIndicator = BytesToInt(4, true);
            if (headerChunkIndicator != (int)MidiDefs.MidiHeaderIdentifier)
            {
                throw new MidiFileException("File does not appear to be a Midi File.", ErrorType.BadMidiFile);
            }
            int headerChunkLength = BytesToInt(4, true);
            if (headerChunkLength != 6)
            {
                throw new MidiFileException("Unsupported Midi header type.", ErrorType.UnsupportedMidi);
            }
            midiFormat = BytesToInt(2, true);
            numberOfTracks = BytesToInt(2, true);
            if (midiFormat == 0 && numberOfTracks != 1)
            {
                throw new MidiFileException("Midi is 'Type 0' but contains more than one track.", ErrorType.BadMidiFile);
            }
            if (numberOfTracks <= 0)
            {
                throw new MidiFileException("Midi file has 0 or less tracks.", ErrorType.BadMidiFile);
            }
            if (midiFormat > 2 || midiFormat < 0)
            {
                throw new MidiFileException("Midi file type is unspoorted.", ErrorType.UnsupportedMidi);
            }
            timeCode = new TimeCode(BytesToInt(2, true));
            if (timeCode.isError())
            {
                throw new MidiFileException("Midi timecode is malformed.", ErrorType.BadMidiFile);
            }

            int trackChunkIndicator = BytesToInt(4, true);
            if (trackChunkIndicator != (int)MidiDefs.TrackHeaderIdentifier)
            {
                throw new MidiFileException("Midi track header is malformed.", ErrorType.BadMidiFile);
            }
            currentTrackLength = BytesToInt(4, true);
            currentTrackPosition = 0;
            int eventTypeBitIdentifier = 0;

            //Begin main parsing loop.
            while (!isFinishedParsing)
            {
                deltaTickTime = ParseVariableIntAndAdvanceCursor();
                currentTickTime += deltaTickTime;
                statusCode = BytesToInt(1, false);

                if (statusCode <= (int)MidiDefs.RunningStatusIDMax)
                {
                    statusCode = previousStatusCode;
                }
                else
                {
                    adjustCursorPosition(1);
                }

                eventTypeBitIdentifier = (statusCode & (int)MidiDefs.MidiStatusEventBits);

                switch (eventTypeBitIdentifier)
                {
                    case (int)MidiDefs.NoteOnIdentifier:
                        NoteOnHandler();
                        break;
                    case (int)MidiDefs.NoteOffIdentifier:
                        NoteOffHandler();
                        break;
                    case (int)MidiDefs.PolyphonicPressureIdentifier:
                        PolyphonicPressureHandler();
                        break;
                    case (int)MidiDefs.ControllerChangeIdentifier:
                        ControllerChangeHandler();
                        break;
                    case (int)MidiDefs.ProgramChangeIdentifier:
                        ProgramChangeHandler();
                        break;
                    case (int)MidiDefs.ChannelPressureIdentifier:
                        ChannelPressureHandler();
                        break;
                    case (int)MidiDefs.PitchBendIdentifier:
                        PitchBendHandler();
                        break;
                    case (int)MidiDefs.SystemMessageIdentifier:
                        SystemMessageHandler();
                        break;
                    case -1:
                        throw new MidiFileException("Running status midi event detected as the first midi event. File is malformed.", ErrorType.BadMidiFile);                       
                    default:
                        throw new MidiFileException("Midi parser failed inside of the status identifier loop. This error should never occur and is normally unreachable.", ErrorType.UnsupportedMidi);
                }
            }
        }


        //File parsing utilities below.
        private int ParseVariableIntAndAdvanceCursor()
        {
            int varInt = 0;
            int numericalBits = 0;
            int controlBits = 0;
            int loopsDone = 0;
            do
            {
                loopsDone++;
                if(loopsDone > 4)
                {
                    throw new MidiFileException("Variable integer is longer than 4 bytes, file is malformed or unsupported." + ErrorType.BadMidiFile);
                }
                numericalBits = fileData[fileDataCursor] & (int)MidiDefs.VariableIntDataBits;
                controlBits = fileData[fileDataCursor] & (int)MidiDefs.VariableIntContinuationFlag;
                varInt <<= 7;
                varInt += numericalBits;
                adjustCursorPosition(1);
            } while (controlBits == (int)MidiDefs.VariableIntContinuationFlag);
            return varInt;
        }

        private int BytesToInt(int bytesToRead, bool adjustCursor)
        {
            if (bytesToRead <= 0)
            {
                throw new ArgumentException("Function Bytes To Int was called with zero or less intended bytes to be read.");
            }

            int parsedInteger = 0;
            for (int i = 1; i <= bytesToRead; i++)
            {
                parsedInteger <<= 8;
                parsedInteger += fileData[i - 1 + fileDataCursor];
            }
            if (adjustCursor) { adjustCursorPosition(bytesToRead); }
            return parsedInteger;
        }

        private int SignedByte(bool adjustCursor)
        {
            if (adjustCursor) { adjustCursorPosition(1); }
            return fileData[fileDataCursor];
        }

        private string BytesToString(int bytesToRead, bool adjustCursor)
        {
            if (bytesToRead <= 0 || fileDataCursor > int.MaxValue)
            {
                throw new ArgumentException();
            }
            int cursorStartPosition = (int)fileDataCursor;
            if (adjustCursor) { adjustCursorPosition(bytesToRead); }

            bool isUnicode = false;
            for (int i = 0; !isUnicode && i < bytesToRead; i++)
            {
                isUnicode = (fileData[cursorStartPosition + i] & (int)MidiDefs.UnicodeIdentifierBits) == (int)MidiDefs.UnicodeIdentifierBits;
            }
            if (isUnicode)
            {
                return System.Text.Encoding.Unicode.GetString(fileData, cursorStartPosition, bytesToRead);
            }
            else
            {
                return System.Text.Encoding.ASCII.GetString(fileData, cursorStartPosition, bytesToRead);
            }
        }

        private void adjustCursorPosition(int amountToMove)
        {
            fileDataCursor += amountToMove;
            currentTrackPosition += amountToMove;
            if (currentTrackPosition >= currentTrackLength)
            {
                //throw new MidiFileException("Track ended, but no end of track event found." + ErrorType.BadMidiFile);
                //TODO debug and fix
            }
            if (fileDataCursor > totalFileSize) 
            {
                throw new MidiFileException("File cursor has moved past the end of the file, file is malformed." + ErrorType.BadMidiFile);
            }
        }
        //end file parsing utilities.


        //Handlers for the midi events are below.
        private void NoteOffHandler()
        {
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int midiChannel = statusCode & (int)MidiDefs.MidiChannelBits;
            int midiNote = BytesToInt(1, true);
            int midiVelocity = BytesToInt(1, true);
            customImpl.NoteOff(currentTimeMS, currentTrackNumber, midiChannel, midiNote, midiVelocity);
            previousStatusCode = statusCode;
        }
        private void NoteOnHandler()
        {
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int midiChannel = statusCode & (int)MidiDefs.MidiChannelBits;
            int midiNote = BytesToInt(1, true);
            int midiVelocity = BytesToInt(1, true);
            customImpl.NoteOn(currentTimeMS, currentTrackNumber, midiChannel, midiNote, midiVelocity);
            previousStatusCode = statusCode;
        }
        private void PolyphonicPressureHandler()
        {
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int midiChannel = statusCode & (int)MidiDefs.MidiChannelBits;
            int midiNote = BytesToInt(1, true);
            int midiPressure = BytesToInt(1, true);
            customImpl.PolyphonicPressure(currentTimeMS, currentTrackNumber, midiChannel, midiNote, midiPressure);
            previousStatusCode = statusCode;
        }
        private void ControllerChangeHandler()
        {
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int midiChannel = statusCode & (int)MidiDefs.MidiChannelBits;
            int midiController = BytesToInt(1, true);
            int midiValue = BytesToInt(1, true);
            customImpl.ControlChange(currentTimeMS, currentTrackNumber, midiChannel, midiController, midiValue);
            previousStatusCode = statusCode;
        }
        private void ProgramChangeHandler()
        {
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int midiChannel = statusCode & (int)MidiDefs.MidiChannelBits;
            int midiProgram = BytesToInt(1, true);
            customImpl.ProgramChange(currentTimeMS, currentTrackNumber, midiChannel, midiProgram);
            previousStatusCode = statusCode;
        }
        private void ChannelPressureHandler()
        {
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int midiChannel = statusCode & (int)MidiDefs.MidiChannelBits;
            int midiPressure = BytesToInt(1, true);
            customImpl.ChannelPolyphonicPressure(currentTimeMS, currentTrackNumber, midiChannel, midiPressure);
            previousStatusCode = statusCode;
        }
        private void PitchBendHandler()
        {
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int midiChannel = statusCode & (int)MidiDefs.MidiChannelBits;
            int midiLSB = BytesToInt(1, true);
            int midiMSB = BytesToInt(1, true);
            int totalBend = (midiMSB << 7) + midiLSB;
            customImpl.PitchBend(currentTimeMS, currentTrackNumber, midiChannel, totalBend);
            previousStatusCode = statusCode;
        }




        private void SystemMessageHandler()
        {
            switch (statusCode)
            {
                case (int)MidiDefs.SysexMessageIdentifier:
                    SysMessageHandler();
                    break;
                case (int)MidiDefs.SysexEscapeIdentifier:
                    if (isUnfinishedSysexMessage) { SysMessageHandler(); }
                    else { SysEscapeHandler(); }
                    break;
                case (int)MidiDefs.MetaEventIdentifier:
                    MetaEventHandler();
                    break;
                default:
                    throw new MidiFileException("Bad Midi system status code encountered.", ErrorType.BadMidiFile);
            }
        }

        private void MetaEventHandler()
        {
            int metaStatus = BytesToInt(1, true);
            switch (metaStatus)
            {
                case (int)MidiDefs.SequenceNumberIdentifier:
                    SequenceNumberHandler();
                    return;
                case (int)MidiDefs.TextIdentifier:
                    TextHandler();
                    return;
                case (int)MidiDefs.CopyrightIdentifier:
                    CopyrightHandler();
                    return;
                case (int)MidiDefs.TrackNameIdentifier:
                    TrackNameHandler();
                    return;
                case (int)MidiDefs.InstrumentNameIdentifier:
                    InstrumentNameHandler();
                    return;
                case (int)MidiDefs.LyricIdentifier:
                    LyricHandler();
                    return;
                case (int)MidiDefs.MarkerIdentifier:
                    MarkerHandler();
                    return;
                case (int)MidiDefs.CuePointIdentifier:
                    CuePointHandler();
                    return;
                case (int)MidiDefs.ProgramNameIdentifier:
                    ProgramNameHandler();
                    return;
                case (int)MidiDefs.DeviceNameIdentifier:
                    DeviceNameHandler();
                    return;
                case (int)MidiDefs.MidiChannelPrefixIdentifier:
                    MidiChannelPrefixHandler();
                    return;
                case (int)MidiDefs.MidiPortIdentifier:
                    MidiPortHandler();
                    return;
                case (int)MidiDefs.EndOfTrackIdentifier:
                    EndOfTrackHandler();
                    return;
                case (int)MidiDefs.TempoIdentifier:
                    TempoHandler();
                    return;
                case (int)MidiDefs.SMPTEOffsetIdentifier:
                    SMPTEOffsetHandler();
                    return;
                case (int)MidiDefs.TimeSignatureIdentifier:
                    TimeSignatureHandler();
                    return;
                case (int)MidiDefs.KeySignatureIdentifier:
                    KeySignatureHandler();
                    return;
                case (int)MidiDefs.SequencerSpecificEventIdentifier:
                    SequencerSpecificEventHandler();
                    return;
                default:
                    UnknownMetaEventHandler();
                    return;
            }
        }

        public void SequenceNumberHandler()
        {
            int eventLength = BytesToInt(1, true);
            if (eventLength == 0 && midiFormat == 2)
            {
                customImpl.SequenceNumber(currentTrackNumber, currentTrackNumber);
                previousStatusCode = statusCode;
                return;
            }
            if(eventLength != 2)
            {
                throw new MidiFileException("Sequence Number event has unexpected length.", ErrorType.BadMidiFile);
            }
            int sequenceNumber = BytesToInt(2, true);
            customImpl.SequenceNumber(currentTrackNumber, sequenceNumber);
            previousStatusCode = statusCode;
        }
        public void TextHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Text event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.Text(currentTimeMS, currentTrackNumber, textData);
            previousStatusCode = statusCode;
        }
        public void CopyrightHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Copyright event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.Copyright(currentTimeMS, currentTrackNumber, textData);
            previousStatusCode = statusCode;
        }
        public void TrackNameHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Track Name event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.TrackName(currentTimeMS, currentTrackNumber, sysExChannel, textData);
            previousStatusCode = statusCode;
        }
        public void InstrumentNameHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Instrument Name event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.InstrumentName(currentTimeMS, currentTrackNumber, sysExChannel, textData);
            previousStatusCode = statusCode;
        }
        public void LyricHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Lyric event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.Lyric(currentTimeMS, currentTrackNumber, sysExChannel, textData);
            previousStatusCode = statusCode;
        }
        public void MarkerHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Marker event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.Marker(currentTimeMS, currentTrackNumber, sysExChannel, textData);
            previousStatusCode = statusCode;
        }
        public void CuePointHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Cue Point event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.CuePoint(currentTimeMS, currentTrackNumber, sysExChannel, textData);
            previousStatusCode = statusCode;
        }
        public void ProgramNameHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Program Name event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.ProgramName(currentTimeMS, currentTrackNumber, sysExChannel, textData);
            previousStatusCode = statusCode;
        }
        public void DeviceNameHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Device Name event has a length that is attempting to read past the end of the file.");
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            string textData = BytesToString(eventLength, true);
            customImpl.DeviceName(currentTimeMS, currentTrackNumber, textData);
            previousStatusCode = statusCode;
        }
        private void MidiChannelPrefixHandler() //Changes the channel for System messages. Internal use only.
        {
            int eventLength = BytesToInt(1, true);
            if(eventLength != 1)
            {
                throw new MidiFileException("Midi Channel Prefix event has unexpected length.", ErrorType.BadMidiFile);
            }
            sysExChannel = BytesToInt(eventLength, true);
            previousStatusCode = statusCode;
        }
        public void MidiPortHandler()
        {
            int eventLength = BytesToInt(1, true);
            if(eventLength!= 1)
            {
                throw new MidiFileException("Midi Port event has unexpected length.", ErrorType.BadMidiFile);
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int portNumber = BytesToInt(eventLength, true);
            customImpl.MidiPort(currentTimeMS, currentTrackNumber, portNumber);
            previousStatusCode = statusCode;
        }
        public void EndOfTrackHandler()
        {
            currentTrackPosition = 0;
            currentTrackNumber++;
            if (midiFormat == 1) { currentTickTime = 0; }
            isUnfinishedSysexMessage = false;
            int eventLength = BytesToInt(1, true);
            if(eventLength != 0)
            {
                throw new MidiFileException("End of Track event has unexpected length.", ErrorType.BadMidiFile);
            }
            if (currentTrackNumber > numberOfTracks)
            {
                isFinishedParsing = true;
                return;
            }

            //The data after an End of Track event should ALWAYS be a track header.
            int trackChunkIdentifier = BytesToInt(4, true);
            if (trackChunkIdentifier != (int)MidiDefs.TrackHeaderIdentifier)
            {
                throw new MidiFileException("Track header is malformed" + ErrorType.BadMidiFile);
            }
            currentTrackLength = BytesToInt(4, true);
            currentTrackPosition = 0;
            previousStatusCode = -1;
        }
        public void TempoHandler()
        {
            if (currentTrackNumber != 1)
            {
                throw new MidiFileException("Tempo data found outside of the first track.", ErrorType.UnsupportedMidi);
            }
            int eventLength = BytesToInt(1, true);
            if (eventLength != 3)
            {
                throw new MidiFileException("Tempo event has malformed length.", ErrorType.BadMidiFile);
            }
            int MicroSecondsPerQuartnerNote = BytesToInt(3, true);
            timeCode.AddTempo(MicroSecondsPerQuartnerNote, currentTickTime);
            if (timeCode.isError())
            {
                throw new MidiFileException("This file somehow managed to provide an out of sequence tempo event. File is malformed.", ErrorType.BadMidiFile);
            }
            previousStatusCode = statusCode;
        }
        public void SMPTEOffsetHandler()
        {
            int eventLength = BytesToInt(1, true);
            if (eventLength != 5)
            {
                throw new MidiFileException("SMPTE Offset event has unexpected length.", ErrorType.BadMidiFile);
            }
            int hourAndFrame = BytesToInt(1, true);
            int hourOnly = hourAndFrame & 0x1f;
            int frameRateBits = hourAndFrame & 0x60;
            int frameRate = 0;
            bool hasDropFrames = false;
            switch (frameRateBits)
            {
                case 0x00:
                    frameRate = 24;
                    break;
                case 0x20:
                    frameRate = 25;
                    break;
                case 0x40:
                    frameRate = 30;
                    hasDropFrames = true;
                    break;
                case 0x60:
                    frameRate = 30;
                    break;
                default:
                    throw new MidiFileException("Parser error in SMPTE offset handler.", ErrorType.UnsupportedMidi);
            }
            int minute = BytesToInt(1, true);
            int second = BytesToInt(1, true);
            int frame = BytesToInt(1, true);
            int fractionalFrame = BytesToInt(1, true);

            if((hourOnly > 23 || hourOnly < 0 || minute > 59 || minute < 0 || second > 59 || second < 0 || 
                frame >= frameRate || frame < 0 || fractionalFrame > 99 ||  fractionalFrame < 0))
            {
                throw new MidiFileException("SMPTE offset event has malformed time data.", ErrorType.BadMidiFile);
            }
            customImpl.SMPTE_Offset(currentTrackNumber, hourOnly, minute, second, frame, fractionalFrame, frameRate, hasDropFrames);
            previousStatusCode = statusCode;
        }
        public void TimeSignatureHandler()
        {
            int eventLength = BytesToInt(1, true);
            if (eventLength != 4)
            {
                throw new MidiFileException("Time Signature event has unexpected length.", ErrorType.BadMidiFile);
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int timeSignatureNumerator = BytesToInt(1, true);
            int denominatorExpononet = BytesToInt(1, true);
            if(denominatorExpononet >= 31) 
            { 
                throw new MidiFileException("Time Signature denominator larger than 2^32. Please reconsider the usage of time signatures larger than two billion.", ErrorType.UnsupportedMidi); 
            }
            int timeSignatureDenominator = (int)BigInteger.Pow(2, denominatorExpononet);
            int midiClocksPerMetronomePulse = BytesToInt(1, true);
            int thirtySecondNotesPerQuarterNote = BytesToInt(1, true);
            customImpl.TimeSignature(currentTimeMS, timeSignatureNumerator, timeSignatureDenominator, midiClocksPerMetronomePulse, thirtySecondNotesPerQuarterNote);
            previousStatusCode = statusCode;
        }
        public void KeySignatureHandler()
        {
            int eventLength = BytesToInt(1, true);
            if (eventLength != 2)
            {
                throw new MidiFileException("Key Signature event has unexpected length.", ErrorType.BadMidiFile);
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            int keySignatureSharps = SignedByte(true);
            bool isMajorKey = (BytesToInt(1, true)) == (int)MidiDefs.KeySignatureMajorFlag;
            if(keySignatureSharps > 14 || keySignatureSharps < -14)
            {
                throw new MidiFileException("This parser does not support key signatures with more than 14 sharps or flats.", ErrorType.UnsupportedMidi);
            }
            customImpl.KeySignature(currentTimeMS, currentTrackNumber, keySignatureSharps, isMajorKey);
            previousStatusCode = statusCode;
        }
        public void SequencerSpecificEventHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Sequencer Specific midi event has a length outside the bounds of the midi file.", ErrorType.BadMidiFile);
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            byte[] rawDataArr = new byte[eventLength];
            Buffer.BlockCopy(fileData, (int)fileDataCursor, rawDataArr, 0, eventLength);
            customImpl.SequencerSpecificEvent(currentTimeMS, currentTrackNumber, rawDataArr, eventLength);
            adjustCursorPosition(eventLength);
            previousStatusCode = statusCode;
        }
        public void UnknownMetaEventHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("Unknown midi event has a length outside the bounds of the midi file.", ErrorType.BadMidiFile);
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            byte [] rawDataArr = new byte[eventLength];
            Buffer.BlockCopy(fileData, (int)fileDataCursor, rawDataArr, 0, eventLength);
            customImpl.UnknownMetaEvent(currentTimeMS, currentTrackNumber, rawDataArr, eventLength);
            adjustCursorPosition(eventLength);
            previousStatusCode = statusCode;
        }


        private void SysMessageHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("System Message midi event has a length outside the bounds of the midi file.", ErrorType.BadMidiFile);
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            byte[] rawDataArr;
            if (eventLength == 0) 
            {
                customImpl.EmptySystemMessage(currentTimeMS, currentTrackNumber);
                previousStatusCode = statusCode;
                return;
            }
            adjustCursorPosition(eventLength - 1);
            int terminalByte = BytesToInt(1, true);
            isUnfinishedSysexMessage = (terminalByte != (int)MidiDefs.SysexEscapeIdentifier);
            adjustCursorPosition(1 - eventLength);

            if (isUnfinishedSysexMessage)
            {
                rawDataArr = new byte[eventLength - 1];
                customImpl.SystemMessage(currentTimeMS, currentTrackNumber, rawDataArr, eventLength, false);
                adjustCursorPosition(eventLength);
            }
            else
            {
                rawDataArr = new byte[eventLength];
                customImpl.SystemMessage(currentTimeMS, currentTrackNumber, rawDataArr, eventLength, true);
                adjustCursorPosition(eventLength);
            }
            previousStatusCode = statusCode;
        }
        private void SysEscapeHandler()
        {
            int eventLength = ParseVariableIntAndAdvanceCursor();
            if (eventLength + fileDataCursor > totalFileSize)
            {
                throw new MidiFileException("System Escape midi event has a length outside the bounds of the midi file.", ErrorType.BadMidiFile);
            }
            int currentTimeMS = timeCode.TickTimeToMiliSeconds(currentTickTime);
            byte[] rawDataArr = new byte[eventLength];
            Buffer.BlockCopy(fileData, (int)fileDataCursor, rawDataArr, 0, eventLength);
            customImpl.SystemEscapeMetaEvent(currentTimeMS, currentTrackNumber, rawDataArr, eventLength);
            adjustCursorPosition(eventLength);
            previousStatusCode = statusCode;
        }
    }
}