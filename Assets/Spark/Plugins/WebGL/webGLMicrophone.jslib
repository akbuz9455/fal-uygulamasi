var sparkLib = {
  // Initialize variables
  $MicrophoneWebGL: {
    audioContext: null,
    mediaStream: null,
    audioSource: null,
    analyser: null,
    buffer: [],
    recorder: null,
    isRecording: false,
  },

  MicrophoneWebGL_Start: async function(deviceName, loop, lengthSec, frequency, channelCount) {

    const constraints = {
      sampleRate: frequency,
      channelCount: channelCount,
      echoCancellation: false,
      autoGainControl: true,
      noiseSuppression: true,
    };

    if (!MicrophoneWebGL.audioContext) {
      MicrophoneWebGL.audioContext = new AudioContext(constraints);
    }

    if (MicrophoneWebGL.isRecording) {
      console.warn('Microphone is already recording.');
      return;
    }

    async function _UpdateData(deviceName) {
      let blob = new Blob(MicrophoneWebGL.chunks, { 'type': 'audio/ogg; codecs=opus' });
      let alignedSize = Math.floor(blob.size / 4) * 4;
      let alignedBlob = await blob.slice(0, alignedSize).arrayBuffer();
      let buf = new Uint8Array(alignedBlob);
      let audioBuffer = await MicrophoneWebGL.audioContext.decodeAudioData(buf.buffer);
      var offlineContext = new OfflineAudioContext(audioBuffer.numberOfChannels, audioBuffer.length, audioBuffer.sampleRate);
      var source = offlineContext.createBufferSource();
      source.buffer = audioBuffer;
      source.connect(offlineContext.destination);
      source.start(0);
  
      let channels = MicrophoneWebGL.channelCount;
      let renderedBuffer = await offlineContext.startRendering();
      let audioAll = _CreateInterleavedBuffer(renderedBuffer);
      console.log(audioAll);
      MicrophoneWebGL.audioAll = audioAll;
    };

    function _CreateInterleavedBuffer(renderedBuffer) {
      let numberOfChannels = MicrophoneWebGL.channelCount;
      let channelData = [];
      let sampleLength = renderedBuffer.length;
      
      // Get channel data
      for (let channel = 0; channel < numberOfChannels; channel++) {
          channelData.push(renderedBuffer.getChannelData(channel));
      }
      
      // Create an interleaved buffer
      let interleavedBuffer = new Float32Array(sampleLength * numberOfChannels);
      
      for (let i = 0; i < sampleLength; i++) {
          for (let channel = 0; channel < numberOfChannels; channel++) {
              interleavedBuffer[i * numberOfChannels + channel] = channelData[channel][i];
          }
      }
      return interleavedBuffer;
    };

    MicrophoneWebGL.channelCount = channelCount;
    MicrophoneWebGL.bufferPosition = 0;
    MicrophoneWebGL.isRecording = true;
    MicrophoneWebGL.chunks = [];

    MicrophoneWebGL.stream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
    const options = {
      channelCount: channelCount,
      mimeType: 'audio/webm;codecs=pcm',
    };

    MicrophoneWebGL.mediaRecorder = new MediaRecorder(MicrophoneWebGL.stream, options);
    MicrophoneWebGL.mediaRecorder.addEventListener("dataavailable", async (e) => {
      MicrophoneWebGL.chunks.push(e.data);
      await _UpdateData(deviceName);
    });
    MicrophoneWebGL.mediaRecorder.start(100);
  },

  MicrophoneWebGL_GetData: function(deviceName, samples, samplesLength, offset) {
    let audioAll = MicrophoneWebGL.audioAll;
    console.log(audioAll, samples, samplesLength, offset);
    var samplesArray = new Float32Array(Module.HEAP8.buffer, samples, samplesLength);
    samplesArray.set(audioAll, offset);
  },

  // Get current recording position
  MicrophoneWebGL_GetPosition: function(deviceName) {
    console.log(MicrophoneWebGL);
    return MicrophoneWebGL.audioAll ? MicrophoneWebGL.audioAll.length : 0;
  },

  // End microphone recording
  MicrophoneWebGL_End: function(deviceName) {
    if (!MicrophoneWebGL.isRecording) {
      console.warn('Microphone is not recording.');
      return;
    }

    MicrophoneWebGL.isRecording = false;
    MicrophoneWebGL.mediaRecorder.stop();
    MicrophoneWebGL.stream.getTracks().forEach(track => track.stop());

  },
};

autoAddDeps(sparkLib, '$MicrophoneWebGL');
mergeInto(LibraryManager.library, sparkLib);
