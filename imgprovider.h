#pragma once

#include <QQuickImageProvider>

class ImgProvider : public QQuickImageProvider {
public:
  ImgProvider();
  ~ImgProvider() = default;

  QPixmap requestPixmap(const QString &id, QSize *size,
                        const QSize &requestedSize) override;
};
