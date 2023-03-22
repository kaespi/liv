#pragma once

#include <QQuickImageProvider>

class ImgProvider : public QQuickImageProvider {
public:
  ImgProvider(const QString &file);
  ~ImgProvider() = default;

  QPixmap requestPixmap(const QString &id, QSize *size,
                        const QSize &requestedSize) override;

private:
  QString m_file{};
};
