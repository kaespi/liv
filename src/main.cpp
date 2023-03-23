#include <QGuiApplication>
#include <QQmlApplicationEngine>

#include "backend/BackendManager.h"

int main(int argc, char *argv[]) {
  QGuiApplication app(argc, argv);
  QQmlApplicationEngine engine;

  QString imgFile{};
  if (argc >= 2) {
    imgFile = argv[1];
  } else {
    imgFile = "../../lightweightimageviewer/data/testimages/bbv1.png";
  }

  BackendManager backendMgr(imgFile);

  engine.addImageProvider("imgprovider", backendMgr.requestImgProvider());

  const QUrl url(u"qrc:/lightweightimageviewer/frontend/main.qml"_qs);
  QObject::connect(
      &engine, &QQmlApplicationEngine::objectCreated, &app,
      [url](QObject *obj, const QUrl &objUrl) {
        if (!obj && url == objUrl)
          QCoreApplication::exit(-1);
      },
      Qt::QueuedConnection);
  engine.load(url);
  return app.exec();
}
