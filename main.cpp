#include <QGuiApplication>
#include <QQmlApplicationEngine>

#include "imgprovider.h"

int main(int argc, char *argv[]) {
  QGuiApplication app(argc, argv);
  QQmlApplicationEngine engine;

  engine.addImageProvider("imgprovider", new ImgProvider);

  const QUrl url(u"qrc:/lightweightimageviewer/main.qml"_qs);
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
