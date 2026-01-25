import { useEffect, useMemo, useState } from "react";
import {
  fetchChapterMetadata,
  fetchChapters,
  fetchConfig,
  getChapterAudioUrl,
} from "./api.js";
import ChapterSelector from "./components/ChapterSelector.jsx";
import IndividualPractice from "./components/IndividualPractice.jsx";
import GroupPractice from "./components/GroupPractice.jsx";

const emptyDefaults = {
  waitMode: "keyboard",
  duration: 20,
  playbackSpeed: 1.5,
  repeatYourShloka: false,
  participants: 4,
  yourTurn: 2,
  stanzaCount: 4,
};

const emptyWaitModes = [
  { id: "keyboard", label: "Keyboard Hit" },
  { id: "duration", label: "Duration" },
];

export default function App() {
  const [chapters, setChapters] = useState([]);
  const [selectedChapterId, setSelectedChapterId] = useState(null);
  const [metadata, setMetadata] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [tab, setTab] = useState("individual");
  const [config, setConfig] = useState({
    defaults: emptyDefaults,
    waitModes: emptyWaitModes,
    playbackSpeed: { min: 0.5, max: 2.0, step: 0.1 },
  });

  useEffect(() => {
    const load = async () => {
      try {
        const [chapterData, configData] = await Promise.all([
          fetchChapters(),
          fetchConfig(),
        ]);
        setChapters(chapterData.chapters || []);
        setConfig((prev) => ({
          ...prev,
          ...configData,
          defaults: { ...prev.defaults, ...(configData.defaults || {}) },
        }));
        if (chapterData.chapters?.length) {
          setSelectedChapterId(chapterData.chapters[0].id);
        }
      } catch (err) {
        setError(err.message || "Failed to load configuration");
      } finally {
        setIsLoading(false);
      }
    };

    load();
  }, []);

  useEffect(() => {
    const loadMetadata = async () => {
      if (selectedChapterId == null) {
        return;
      }
      setError("");
      try {
        const data = await fetchChapterMetadata(selectedChapterId);
        setMetadata(data);
      } catch (err) {
        setError(err.message || "Failed to load chapter metadata");
        setMetadata(null);
      }
    };

    loadMetadata();
  }, [selectedChapterId]);

  const chapter = useMemo(
    () => chapters.find((item) => item.id === selectedChapterId),
    [chapters, selectedChapterId]
  );

  const audioUrl = selectedChapterId == null ? "" : getChapterAudioUrl(selectedChapterId);

  return (
    <div className="app">
      <header>
        <div>
          <h1>Gita Practice</h1>
          <p>Structured Bhagavad Gita recitation with individual and group modes.</p>
        </div>
        <div className="tabs">
          <button
            className={tab === "individual" ? "active" : ""}
            onClick={() => setTab("individual")}
          >
            Individual Practice
          </button>
          <button
            className={tab === "group" ? "active" : ""}
            onClick={() => setTab("group")}
          >
            Group Practice
          </button>
        </div>
      </header>

      {error ? <div className="error">{error}</div> : null}

      <ChapterSelector
        chapters={chapters}
        selectedChapterId={selectedChapterId}
        onChange={setSelectedChapterId}
        isLoading={isLoading}
      />

      {tab === "individual" ? (
        <IndividualPractice
          chapter={chapter}
          audioUrl={audioUrl}
          metadata={metadata}
          waitModes={config.waitModes}
          defaults={config.defaults}
          speedConfig={config.playbackSpeed}
        />
      ) : (
        <GroupPractice
          chapter={chapter}
          audioUrl={audioUrl}
          metadata={metadata}
          waitModes={config.waitModes}
          defaults={config.defaults}
          speedConfig={config.playbackSpeed}
        />
      )}
    </div>
  );
}
