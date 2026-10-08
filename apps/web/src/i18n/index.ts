import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import tr from "./tr.json";

// Turkish only for now; resources are bundled so no network request is needed.
void i18n.use(initReactI18next).init({
  resources: { tr: { translation: tr } },
  lng: "tr",
  fallbackLng: "tr",
  interpolation: { escapeValue: false },
});

export default i18n;
